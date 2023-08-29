using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data
{
    public class DbInitializer
    {
        public static void InitDb(WebApplication app)
        {
            using IServiceScope scope = app.Services.CreateScope();

            SeedData(scope.ServiceProvider.GetService<AuctionDbContext>());
        }

        private static void SeedData(AuctionDbContext context)
        {
            context.Database.Migrate();

            if (context.Auctions.Any())
            {
                Console.WriteLine("Already Existed, no need to seed data");
                return;
            }

            var auction = new List<Auction>() 
            { 
                new Auction()
                {
                    Id = Guid.NewGuid(),
                    Status = Status.Live,
                    Seller = "bob",
                    AuctionEnd = DateTime.UtcNow.AddDays(30),
                    Item = new Item()
                    {
                        Make = "Ford",
                        Model = "Ford Model 1",
                        Color = "Black",
                        Mileage = 15657,
                        Year = 2023,
                        ImageUrl = "https://images.wapcar.my/file1/dfe0c5b8cfbe4f8295035b0346011e8c_912x516.jpg"
                    }
                },
                new Auction()
                {
                    Id = Guid.NewGuid(),
                    Status = Status.Live,
                    Seller = "philips",
                    AuctionEnd = DateTime.UtcNow.AddDays(3),
                    Item = new Item()
                    {
                        Make = "Toyota",
                        Model = "Toyota Camry",
                        Color = "White",
                        Mileage = 6765,
                        Year = 2023,
                        ImageUrl = "https://paultan.org/image/2022/02/2022-Toyota-Camry-facelift-Malaysia-dynamic-photo-1-630x354.jpg"
                    }
                },
                new Auction()
                {
                    Id = Guid.NewGuid(),
                    Status = Status.Live,
                    Seller = "philips",
                    AuctionEnd = DateTime.UtcNow.AddDays(-3),
                    Item = new Item()
                    {
                        Make = "Hyundai",
                        Model = "Hyundai Grand i10 Nios",
                        Color = "White",
                        Mileage = 65,
                        Year = 2023,
                        ImageUrl = "https://www.hyundai.com.my/showroom/ioniq5/assets/teaser/ioniq5-ne-highlight-kv-pc.jpg"
                    }
                }
            };

            context.AddRange(auction);

            context.SaveChanges();
        }
    }
}
