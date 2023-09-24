using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
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
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
      this._context = context;
      this._mapper = mapper;
      this._publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions(string date)
    {
      var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

      if (!string.IsNullOrEmpty(date))
      {
        query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
      }

      //List<Auction> auctions = await this._context.Auctions
      //    .Include(x => x.Item)
      //    .OrderBy(x => x.Item.Make)
      //    .ToListAsync();

      //return _mapper.Map<List<AuctionDto>>(auctions);

      return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
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

      AuctionDto newAuction = _mapper.Map<AuctionDto>(auction);

      await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

      bool result = await this._context.SaveChangesAsync() > 0;

      if (!result) return BadRequest("Could not save changes to DB");

      return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, newAuction);
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

      await _publishEndpoint.Publish<AuctionUpdated>(_mapper.Map<AuctionUpdated>(auction));

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

      await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

      bool result = await this._context.SaveChangesAsync() > 0;

      if (!result) return BadRequest("Could not update DB");

      return Ok();
    }
  }
}
