﻿using System.Threading.Tasks;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFramework.Exceptions.Tests
{
    public abstract class DatabaseTests
    {
        internal DemoContext Context { get; }

        protected DatabaseTests(DemoContext context)
        {
            Context = context;
        }

        [Fact]
        public virtual async Task UniqueColumnViolationThrowsUniqueConstraintException()
        {
            Context.Products.Add(new Product { Name = "GD" });
            Context.Products.Add(new Product { Name = "GD" });

            Assert.Throws<UniqueConstraintException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<UniqueConstraintException>(() => Context.SaveChangesAsync());
        }

        [Fact]
        public virtual async Task PrimaryKeyViolationThrowsUniqueConstraintException()
        {
            var product1 = new Product { Name = "GD", Id = 42 };
            var product2 = new Product { Name = "GD", Id = 42 };

            Context.Products.Add(product1);
            Context.SaveChanges();
            Context.Entry(product1).State = EntityState.Detached;

            Context.Products.Add(product2);
            Assert.Throws<UniqueConstraintException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<UniqueConstraintException>(() => Context.SaveChangesAsync());
        }

        [Fact]
        public virtual async Task RequiredColumnViolationThrowsCannotInsertNullException()
        {
            Context.Products.Add(new Product());

            Assert.Throws<CannotInsertNullException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<CannotInsertNullException>(() => Context.SaveChangesAsync());
        }

        [Fact]
        public virtual async Task MaxLengthViolationThrowsMaxLengthExceededException()
        {
            Context.Products.Add(new Product { Name = new string('G', 20) });

            Assert.Throws<MaxLengthExceededException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<MaxLengthExceededException>(() => Context.SaveChangesAsync());
        }

        [Fact]
        public virtual async Task NumericOverflowViolationThrowsNumericOverflowException()
        {
            var product = new Product { Name = "GD" };
            Context.Products.Add(product);
            Context.ProductSales.Add(new ProductSale { Price = 3141.59265m, Product = product });

            Assert.Throws<NumericOverflowException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<NumericOverflowException>(() => Context.SaveChangesAsync());
        }

        [Fact]
        public virtual async Task ReferenceViolationThrowsReferenceConstraintException()
        {
            Context.ProductSales.Add(new ProductSale { Price = 3.14m });

            Assert.Throws<ReferenceConstraintException>(() => Context.SaveChanges());
            await Assert.ThrowsAsync<ReferenceConstraintException>(() => Context.SaveChangesAsync());
        }

        public virtual void Dispose()
        {
            foreach (var entityEntry in Context.ChangeTracker.Entries())
            {
                entityEntry.State = EntityState.Detached;
            }

            Context.Products.RemoveRange(Context.Products);
            Context.SaveChanges();
        }
    }
}