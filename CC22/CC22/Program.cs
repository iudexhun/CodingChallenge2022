using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using Newtonsoft.Json;


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
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(getMenus(ref data), Formatting.Indented));
        Console.WriteLine();
        Console.WriteLine(lwProfitSumed(ref data));

    }

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
    public static Menus getMenus(ref Rootobject data)
    {
        Menus menus = new Menus();

        foreach (var r in data.recipes)
        {
            if (r.glutenFree && r.lactoseFree)
            {
                menus.lactoseAndGlutenFree.Add(new(r.name, r.price));
                continue;
            }
            if (r.glutenFree)
            {
                menus.glutenFree.Add(new(r.name, r.price));
                continue;
            }
            if (r.lactoseFree)
            {
                menus.lactoseFree.Add(new(r.name, r.price));
                continue;
            }

        }

        return menus;
    }
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
                usedIngs[i.name] += int.Parse(i.amount.Split(' ')[0])*s.amount;
               
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

public struct amountandmeasure
{
    public int amount;
    public string measure;
}