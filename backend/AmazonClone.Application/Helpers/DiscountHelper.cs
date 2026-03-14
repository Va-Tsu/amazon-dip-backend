using AmazonClone.Domain.Entities;

namespace AmazonClone.Application.Helpers;

public static class DiscountHelper
{
    public static void ApplyDiscount(Product product)
    {
        var now = DateTime.UtcNow;
        var activeDiscount = product.Discounts
            .Where(d => d.IsActive && d.StartAt <= now && d.EndAt >= now)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();
        
        if (activeDiscount == null)
        {
            product.CurrentPrice = product.Price;
            product.OldPrice = null;
            product.HasDiscount = false;

            return;
        }

        if (activeDiscount.DiscountPrice.HasValue)
        {
            product.CurrentPrice = activeDiscount.DiscountPrice.Value;
            product.OldPrice = product.Price;
            product.HasDiscount = true;
            return;
        }

        if (activeDiscount.DiscountPersentage.HasValue)
        {
            product.CurrentPrice = product.Price -
                                   (product.Price*activeDiscount.DiscountPersentage.Value/100m);
            product.OldPrice = product.Price;
            product.HasDiscount = true;
            return;
        }
        
        product.CurrentPrice = product.Price;
        product.OldPrice = null;
        product.HasDiscount = false;
    }
    
    
    /*
    public static Discount? GetActiveDiscount(Product product)
    {
        var now = DateTime.Now;
        return product.Discounts.FirstOrDefault(d=>
            d.IsActive &&
            d.StartAt <=now &&
            d.EndAt >= now);
    }

    public static decimal GetCurrentPrice(Product product)
    {
        var discount = GetActiveDiscount(product);
        if (discount == null)
        {
            return product.Price;
        }

        if (discount.DiscountPrice.HasValue)
        {
            return discount.DiscountPrice.Value;
        }

        if (discount.DiscountPersentage.HasValue)
        {
            return product.Price - (product.Price * discount.DiscountPersentage.Value/100m);
        }

        return product.Price;
    }

    public static int? GetDiscountPercentage(Product product)
    {
        var discount = GetActiveDiscount(product);
        if (discount == null)
        {
            return null;
        }

        if (discount.DiscountPersentage.HasValue)
        {
            return discount.DiscountPersentage.Value;
        }

        if (discount.DiscountPrice.HasValue && product.Price > 0)
        {
            return (int)Math.Round((product.Price - discount.DiscountPrice.Value) / product.Price - 100m);
        }

        return null;
    } */
}