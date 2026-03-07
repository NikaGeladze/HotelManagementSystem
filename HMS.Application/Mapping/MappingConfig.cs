using HMS.Application.Models.DTOs.Auth;
using HMS.Application.Models.DTOs.Hotel;
using HMS.Application.Models.DTOs.Reservation;
using HMS.Application.Models.DTOs.Room;
using HMS.Domain.Entities;
using Mapster;

namespace HMS.Application.Mapping;

public static class MappingConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<Hotel, HotelSummaryDto>()
            .Map(dest => dest.RoomCount, src => src.Rooms.Count)
            .Map(dest => dest.ManagerCount,src => src.Managers.Count);

        config.NewConfig<ApplicationUser, ManagerResponseDto>();
        
        config.NewConfig<Hotel, HotelDetailDto>().Map(dest => dest.Managers,src => src.Managers);

        config.NewConfig<CreateHotelDto, Hotel>();
        config.NewConfig<UpdateHotelDto, Hotel>();
        
        config.NewConfig<Room, RoomResponseDto>()
            .Map(dest => dest.HotelName, src => src.Hotel.Name);
        config.NewConfig<CreateRoomDto, Room>();
        //config.NewConfig<UpdateRoomDto, Room>();
        config.NewConfig<UpdateRoomDto, Room>()
            .IgnoreNullValues(true);
        config.NewConfig<Reservation, ReservationResponseDto>()
            .Map(dest => dest.GuestFullName, src => $"{src.Guest.FirstName} {src.Guest.LastName}")
            .Map(dest => dest.Rooms, src => src.ReservationRooms.Select(rr => rr.Room));
    }
}