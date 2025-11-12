using System;
using AutoMapper;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.DTOs.Trabajadores;
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

        CreateMap<TrabajadorCrearDTO, Trabajador>();
        CreateMap<HorarioDTO, Horario>();
        CreateMap<Horario, HorarioDTO>();

        CreateMap<Trabajador, TrabajadoresListarDTO>()
            .ForMember(dest => dest.Horarios, opt => opt.MapFrom(src => src.Horarios));

        CreateMap<Trabajador, TrabajadoresListarDTO>().ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Persona.CarnetIdentidad)).ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Persona.ApellidoPaterno + " " + src.Persona.ApellidoMaterno)).ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Persona.PrimerNombre + " " + src.Persona.SegundoNombre)).ForMember(dest => dest.IdNacionalidad, opt => opt.MapFrom(src => src.Persona.IdNacionalidad)).ForMember(dest => dest.Nacionalidad, opt => opt.MapFrom(src => src.Persona.Nacionalidad.ValorCategoria)).ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.Persona.Genero)).ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Cargo.NombreCargo)).ForMember(dest => dest.NombreOficina, opt => opt.MapFrom(src => src.Cargo.Oficina.Nombre)).ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstadoTrabajador));

        CreateMap<Rol, RolDTO>();
    }
}
