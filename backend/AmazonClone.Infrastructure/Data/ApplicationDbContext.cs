using AmazonClone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AmazonClone.Infrastructure.Data;

public class ApplicationDbContext: IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<RevokedToken> RevokedTokens { get; set; }
    
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Cart>Carts{ get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<ProductComment> ProductComments { get; set; }
    public DbSet<Seller> Sellers { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<ProductParameter> ProductParameters { get; set; }

    public DbSet<RecentlyViewedProduct> RecentlyViewedProducts { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Product>()
            .HasOne(p => p.Country)
            .WithMany(p=>p.Products)
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CartItem>()
            .HasOne(c => c.Cart)
            .WithMany(i=>i.Items)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<CartItem>()
            .HasOne(c => c.Product)
            .WithMany(i=>i.CartItems)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<OrderItem>()
            .HasOne(oi=>oi.Order)
            .WithMany(o=>o.Items)
            .HasForeignKey(oi=>oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<OrderItem>()
            .HasOne(oi=>oi.Product)
            .WithMany(p=>p.OrderItems)
            .HasForeignKey(oi=>oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<OrderItem>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        
        builder.Entity<Order>()
            .Property(p => p.TotalPrice)
            .HasPrecision(18, 2);
        
        builder.Entity<Cart>()
            .Property(p => p.TotalPrice)
            .HasPrecision(18, 2);
        
        builder.Entity<CartItem>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Entity<RecentlyViewedProduct>()
            .HasOne(r => r.User)
            .WithMany(u => u.RecentlyViewedProducts)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecentlyViewedProduct>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecentlyViewedProduct>()
            .HasIndex(r => new { r.UserId, r.ViewedAt });
        builder.Entity<RecentlyViewedProduct>()
            .HasIndex(r => new { r.UserId, r.ProductId });


        builder.Entity<Product>()
            .HasMany(p => p.Discounts)
            .WithOne(d => d.Product)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);
        
        builder.Entity<Product>()
            .Property(p => p.OldPrice)
            .HasPrecision(18, 2);
        
        builder.Entity<Product>()
            .Property(p => p.Weight)
            .HasPrecision(18, 2);
        
        builder.Entity<Product>()
            .Property(p => p.ParcelWeight)
            .HasPrecision(18, 2);
        
        builder.Entity<Discount>()
            .Property(d=>d.DiscountPrice)
            .HasPrecision(18, 2);
        
        builder.Entity<Discount>()
            .HasIndex(d => d.ProductId);
        
        builder.Entity<Discount>()
            .HasIndex(d => new{d.IsActive, d.StartAt, d.EndAt});

        builder.Entity<ProductComment>()
            .HasOne(c => c.Product)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<ProductComment>()
            .HasOne(c => c.User)
            .WithMany(u=>u.ProductComments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<ProductComment>()
            .HasIndex(c=>c.ProductId);

        builder.Entity<ProductComment>()
            .HasIndex(c => c.UserId);
        
        
        builder.Entity<User>()
            .HasOne(u=>u.Seller)
            .WithOne(u=>u.User)
            .HasForeignKey<Seller>(u=>u.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<Product>()
            .HasOne(p => p.Seller)
            .WithMany(p=>p.Products)
            .HasForeignKey(p=>p.SellerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        /*builder.Entity<Seller>()
            .HasOne(s => s.Country)
            .WithMany(u => u.Sellers)
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);*/
      
        builder.Entity<Seller>()
            .Property(s=>s.Balance)
            .HasPrecision(18, 2);
        
        builder.Entity<Seller>()
            .Property(s=>s.PandingBalance)
            .HasPrecision(18, 2);

        builder.Entity<Product>()
            .HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Product>()
            .HasMany(p=>p.Parameters)
            .WithOne(pp=>pp.Product)
            .HasForeignKey(pp=>pp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProductImage>()
            .Property(i => i.Url)
            .HasMaxLength(500)
            .IsRequired();
        builder.Entity<ProductImage>()
            .HasIndex(i => i.ProductId);
        builder.Entity<ProductImage>()
            .HasIndex(i=>new{i.ProductId, i.SortOrder});
        builder.Entity<ProductParameter>()
            .Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Entity<ProductParameter>()
            .Property(p => p.Value)
            .HasMaxLength(500)
            .IsRequired();
        builder.Entity<ProductParameter>()
            .HasIndex(p => p.ProductId);





    }
    
}