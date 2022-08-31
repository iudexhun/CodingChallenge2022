using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using Newtonsoft.Json;

//I made a class for all the objects, so that it is always self-explainatory what is what.
//please note that this solution was done in a hurry, but I tried to keep it simple.
public class Rootobject
{
    public Recipe[] recipes { get; set; }
    public Inventory[] inventory { get; set; }
    public Salesoflastweek[] salesOfLastWeek { get; set; }
    public Wholesaleprice[] wholesalePrices { get; set; }
}

public class Recipe
{
    public string name { get; set; }
    public string price { get; set; }
    public bool lactoseFree { get; set; }
    public bool glutenFree { get; set; }
    public Ingredient[] ingredients { get; set; }
}

public class Ingredient
{
    public string name { get; set; }
    public string amount { get; set; }
}

public class Inventory
{
    public string name { get; set; }
    public string amount { get; set; }
}

public class Salesoflastweek
{
    public string name { get; set; }
    public int amount { get; set; }
}

public class Wholesaleprice
{
    public string name { get; set; }
    public string amount { get; set; }
    public int price { get; set; }
}

public class MenuItem
{
    public string name;
    public string price;

    public MenuItem(string name, string price)
    {
        this.name = name;
        this.price = price;
    }
}

public class Menus
{
    public List<MenuItem> glutenFree = new();
    public List<MenuItem> lactoseFree = new();
    public List<MenuItem> lactoseAndGlutenFree = new();

    public Menus(ref Rootobject data)
    {
        foreach (var r in data.recipes)
        {
            if (r.glutenFree && r.lactoseFree)
            {
                lactoseAndGlutenFree.Add(new(r.name, r.price));
                continue;
            }
            if (r.glutenFree)
            {
                glutenFree.Add(new(r.name, r.price));
                continue;
            }
            if (r.lactoseFree)
            {
                lactoseFree.Add(new(r.name, r.price));
                continue;
            }

        }


    }
}

public class MaxPossibleItem
{
    public string name;
    public int amount;

    public MaxPossibleItem(string name, int amount)
    {
        this.name = name;
        this.amount = amount;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        StreamReader sr = new("bakery.json");
        string jsonInput = sr.ReadToEnd();
        sr.Close();

        var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(jsonInput);

        Console.WriteLine(lwSalesSumed(ref data));
        Console.WriteLine();
        Menus allergicMenus = new(ref data);
        StreamWriter sw = new("menus.json");
        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(allergicMenus, Formatting.Indented));
        sw.Close();
        Console.WriteLine(lwProfitSumed(ref data));
        Console.WriteLine();
        sw = new("maxes.json");
        sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(maxPossibles(ref data), Formatting.Indented));
        sw.Close();
        string jsonGodFather = "[\r\n  {\r\n    \"name\": \"Francia krémes\",\r\n    \"amount\": 300\r\n  },\r\n  {\r\n    \"name\": \"Rákóczi túrós\",\r\n    \"amount\": 200\r\n  },\r\n  {\r\n    \"name\": \"Képviselőfánk\",\r\n    \"amount\": 300\r\n  },\r\n  {\r\n    \"name\": \"Isler\",\r\n    \"amount\": 100\r\n  },\r\n  {\r\n    \"name\": \"Tiramisu\",\r\n    \"amount\": 150\r\n  }\r\n]";
        Console.WriteLine(GodFathersBlessing(jsonGodFather, ref data));
    }

  //returns how much money it takes to buy all the necessary things for the daughter's wedding
    public static int GodFathersBlessing(string jsonIn, ref Rootobject data)
    {
        var GFOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GodFatherOrderItem>>(jsonIn);
        int wholesaleorderFullPrice = 0;
        foreach(var o in GFOrder)
        {
            int price = 0;
            foreach (var i in data.recipes.FirstOrDefault(x => x.name == o.name).ingredients)
            {

                int necessaryamount = int.Parse(i.amount.Split(' ')[0])*o.amount;
                if (i.amount.Split(' ')[1] == "g" || i.amount.Split(' ')[1] == "ml")
                {
                    necessaryamount = necessaryamount / 1000;
                }

                int necessaryBulk = necessaryamount /
                    int.Parse(data.wholesalePrices.FirstOrDefault(x => x.name == i.name).amount.Split(' ')[0]);
                if (necessaryamount % int.Parse(data.wholesalePrices.FirstOrDefault(x => x.name == i.name).amount.Split(' ')[0]) != 0)
                {
                    necessaryBulk++;
                }

                price +=  necessaryBulk * data.wholesalePrices.FirstOrDefault(x => x.name == i.name).price;
            }
            wholesaleorderFullPrice += price;
        }

        return wholesaleorderFullPrice;
    }
//returns an array of the items that can be made with the current inventory, and also how much of those items could be made. The items are ordered by name, asc.
  
    public static MaxPossibleItem[] maxPossibles(ref Rootobject data)
    {
        Dictionary<string, int> convInventory = new(); //store in ml, g, and pc
        foreach (var i in data.inventory)
        {
            if (!convInventory.ContainsKey(i.name))
            {
                convInventory.Add(i.name, 0);
            }

            if (i.amount.Split(' ')[1] == "kg" || i.amount.Split(' ')[1] == "l")
            {
                convInventory[i.name] = int.Parse(i.amount.Split(' ')[0]) * 1000;
            }
            else
            {
                convInventory[i.name] = int.Parse(i.amount.Split(' ')[0]);
            }

        }
        List<MaxPossibleItem> maxPossibles = new();
        foreach (var r in data.recipes)
        {
            int minMax = convInventory[r.ingredients[0].name] / int.Parse(r.ingredients[0].amount.Split(' ')[0]);
            foreach (var i in r.ingredients)
            {
                if (convInventory[i.name] / int.Parse(i.amount.Split(' ')[0]) < minMax)
                {
                    minMax = convInventory[i.name] / int.Parse(i.amount.Split(' ')[0]);
                }
            }
            if (minMax > 0)
            {
                maxPossibles.Add(new(r.name, minMax));
            }


        }

        return maxPossibles.OrderBy(x => x.name).ToArray();

    }
// returns last week's sales, sumed.
    public static int lwSalesSumed(ref Rootobject data)
    {
        int salesSum = 0;
        foreach (var d in data.salesOfLastWeek)
        {
            int price = int.Parse(data.recipes.FirstOrDefault(x => x.name == d.name).price.Split(' ')[0]);
            salesSum += (price * d.amount);
        }
        return salesSum;
    }

  // returns last week's profit, sumed.
    public static double lwProfitSumed(ref Rootobject data)
    {

        Dictionary<string, double> wsPriceScaled = new();
        foreach (var wsp in data.wholesalePrices)
        {
            if (!wsPriceScaled.ContainsKey(wsp.name))
            {
                wsPriceScaled.Add(wsp.name, 0);
            }
            double scaledPrice = wsp.price / int.Parse(wsp.amount.Split(' ')[0]); // scaled to 1 unit

            if (wsp.amount.Split(' ')[1] == "kg" || wsp.amount.Split(' ')[1] == "l")
            {
                scaledPrice = scaledPrice / 1000; //scaled to gs/mls price
            }
            wsPriceScaled[wsp.name] = scaledPrice; // scaled to 1 pc/ml/g
        }

        Dictionary<string, int> usedIngs = new();
        foreach (var s in data.salesOfLastWeek)
        {
            var recipe = data.recipes.FirstOrDefault(x => x.name == s.name);
            foreach (var i in recipe.ingredients)
            {
                if (!usedIngs.ContainsKey(i.name))
                {
                    usedIngs.Add(i.name, 0);
                }
                usedIngs[i.name] += int.Parse(i.amount.Split(' ')[0]) * s.amount;

            }
        }

        double ingPriceSum = 0;

        foreach (var u in usedIngs)
        {
            ingPriceSum += u.Value * wsPriceScaled[u.Key];
        }

        return lwSalesSumed(ref data) - ingPriceSum;
    }
}

public class GodFatherOrderItem
{
    public string name { get; set; }
    public int amount { get; set; }
}
