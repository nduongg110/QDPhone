using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QDPhone.Web.Models.Entities;
using QDPhone.Web.Models.Identity;

namespace QDPhone.Web.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<AdminAuditLog> AdminAuditLogs => Set<AdminAuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Coupon>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<CouponUsage>().HasIndex(x => new { x.CouponId, x.UserId, x.OrderId }).IsUnique();
        builder.Entity<PaymentTransaction>().HasIndex(x => x.ExternalTransactionId).IsUnique();
        builder.Entity<AdminAuditLog>().HasIndex(x => x.CreatedAt);
        builder.Entity<Order>()
            .HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<CouponUsage>()
            .HasOne<Coupon>()
            .WithMany()
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<PaymentTransaction>()
            .HasOne<Order>()
            .WithMany()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<OrderItem>()
            .HasOne(x => x.Order)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<OrderItem>()
            .HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ProductImage>().HasIndex(x => new { x.ProductId, x.SortOrder }).IsUnique();
        builder.Entity<ProductImage>()
            .HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProductVariant>().Property(x => x.Price).HasPrecision(18, 2);
        builder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Entity<Order>().Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Entity<Coupon>().Property(x => x.Value).HasPrecision(18, 2);
        builder.Entity<Coupon>().Property(x => x.MaxDiscount).HasPrecision(18, 2);
        builder.Entity<Coupon>().Property(x => x.MinOrderAmount).HasPrecision(18, 2);
    }
}
