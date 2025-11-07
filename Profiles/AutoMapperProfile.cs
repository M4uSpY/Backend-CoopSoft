using System;
using AutoMapper;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Models;

namespace BackendCoopSoft.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // indicamos que queremos convertir el objeto Usuario a un UserDto
        CreateMap<Usuario, UsuarioListarDTO>().ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.Persona.ApellidoPaterno + " " + src.Persona.ApellidoMaterno + " " + src.Persona.SegundoNombre + " " + src.Persona.PrimerNombre)).ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.NombreRol)).ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Persona.CarnetIdentidad)).ForMember(dest => dest.DescripcionRol, opt => opt.MapFrom(src => src.Rol.Descripcion)).ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.Persona.Genero)).ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstadoUsuario)).ForMember(dest => dest.IdPersona, opt => opt.MapFrom(src => src.Persona.IdPersona));
        CreateMap<UsuarioCrearDTO, Usuario>();

        CreateMap<Persona, PersonasListarDTO>();
        CreateMap<PersonaCrearDTO, Persona>().ReverseMap();

        CreateMap<Rol, RolDTO>();
    }
}
