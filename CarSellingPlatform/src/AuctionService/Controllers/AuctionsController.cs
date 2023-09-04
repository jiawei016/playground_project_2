using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
  [ApiController]
  [Route("api/auctions")]
  public class AuctionsController : ControllerBase
  {
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
      this._context = context;
      this._mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
      List<Auction> auctions = await this._context.Auctions
          .Include(x => x.Item)
          .OrderBy(x => x.Item.Make)
          .ToListAsync();

      return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
      Auction auction = await this._context.Auctions
          .Include(x => x.Item)
          .FirstOrDefaultAsync(x => x.Id == id);

      if (auction == null) return NotFound();

      return this._mapper.Map<AuctionDto>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    {
      Auction auction = this._mapper.Map<Auction>(auctionDto);
      //TODO: add current user as seller
      auction.Seller = "test";

      _context.Auctions.Add(auction);

      bool result = await this._context.SaveChangesAsync() > 0;

      if(!result) return BadRequest("Could not save changes to DB");

      return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, this._mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuctionDto)
    {
      Auction auction = await this._context.Auctions.Include(x => x.Item)
        .FirstOrDefaultAsync(x => x.Id == id);

      if (auction == null) return NotFound();

      // TODO: Check seller == username

      auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
      auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
      auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
      auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
      auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

      bool result = await this._context.SaveChangesAsync() > 0;

      if (result) return Ok();

      return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
      Auction auction = await this._context.Auctions.FindAsync(id);

      if (auction == null) return NotFound();

      // TODO : check seller == username

      this._context.Auctions.Remove(auction);

      bool result = await this._context.SaveChangesAsync() > 0;

      if (!result) return BadRequest("Could not update DB");

      return Ok();
    }
  }
}
