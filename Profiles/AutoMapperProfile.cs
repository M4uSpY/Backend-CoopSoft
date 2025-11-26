using System;
using AutoMapper;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Asistencia;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.DTOs.Faltas;
using BackendCoopSoft.DTOs.Historicos;
using BackendCoopSoft.DTOs.InformacionPersonal;
using BackendCoopSoft.DTOs.InformacionPersonal.Contratacion;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica.Capacitaciones;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.DTOs.Trabajadores;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Models;

namespace BackendCoopSoft.Profiles;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<LogAcceso, LogsAccesoDTO>().ForMember(dest => dest.ApellidosNombres, opt => opt.MapFrom(src => src.Usuario.Persona.ApellidoPaterno + " " + src.Usuario.Persona.ApellidoMaterno + " " + src.Usuario.Persona.SegundoNombre + " " + src.Usuario.Persona.PrimerNombre)).ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.NombreUsuario)).ForMember(dest => dest.FechaLogin, opt => opt.MapFrom(src => src.FechaLogin)).ForMember(dest => dest.HoraLogin, opt => opt.MapFrom(src => src.HoraLogin)).ForMember(dest => dest.FechaLogout, opt => opt.MapFrom(src => src.FechaLogout)).ForMember(dest => dest.HoraLogout, opt => opt.MapFrom(src => src.HoraLogout));


        // indicamos que queremos convertir el objeto Usuario a un UserDto
        CreateMap<Usuario, UsuarioListarDTO>().ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.Persona.ApellidoPaterno + " " + src.Persona.ApellidoMaterno + " " + src.Persona.SegundoNombre + " " + src.Persona.PrimerNombre)).ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol.NombreRol)).ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Persona.CarnetIdentidad)).ForMember(dest => dest.DescripcionRol, opt => opt.MapFrom(src => src.Rol.Descripcion)).ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.Persona.Genero)).ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstadoUsuario)).ForMember(dest => dest.IdPersona, opt => opt.MapFrom(src => src.Persona.IdPersona));
        CreateMap<UsuarioCrearDTO, Usuario>();
        CreateMap<UsuarioActualizarDTO, Usuario>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());


        CreateMap<PersonaCrearDTO, Persona>().ReverseMap();
        CreateMap<Persona, PersonasListarDTO>();





        CreateMap<Trabajador, TrabajadorPersonaDTO>()
            .ForMember(
                dest => dest.TituloObtenido,
                opt => opt.MapFrom(src =>
                    src.FormacionesAcademicas
                        .OrderByDescending(f => f.AnioGraduacion)
                        .Select(f => f.TituloObtenido)
                        .FirstOrDefault()
                )
            )
            .ForMember(
                dest => dest.Formaciones,
                opt => opt.MapFrom(src => src.FormacionesAcademicas)
            )
            .ForMember(
                dest => dest.Capacitaciones,
                opt => opt.MapFrom(src => src.Capacitaciones)
            );

        CreateMap<FormacionAcademica, FormacionAcademicaResumenDTO>();
        CreateMap<FormacionAcademica, FormacionAcademicaEditarDTO>();
        CreateMap<FormacionAcademicaCrearDTO, FormacionAcademica>().ForMember(d => d.IdFormacion, opt => opt.Ignore());
        CreateMap<FormacionAcademicaEditarDTO, FormacionAcademica>().ForMember(d => d.IdFormacion, opt => opt.Ignore());

        CreateMap<AsistenciaCrearDTO, Asistencia>();


        CreateMap<Capacitacion, CapacitacionResumenDTO>();
        CreateMap<Capacitacion, CapacitacionEditarDTO>();
        CreateMap<CapacitacionCrearDTO, Capacitacion>().ForMember(d => d.IdCapacitacion, opt => opt.Ignore());
        CreateMap<CapacitacionEditarDTO, Capacitacion>().ForMember(d => d.IdCapacitacion, opt => opt.Ignore());

        CreateMap<Contrato, ContratoDTO>();
        CreateMap<Contrato, ContratoActualizarDTO>();
        CreateMap<ContratoActualizarDTO, Contrato>().ForMember(d => d.IdContrato, opt => opt.Ignore());



        CreateMap<TrabajadorCrearDTO, Trabajador>();
        CreateMap<HorarioDTO, Horario>();
        CreateMap<Horario, HorarioDTO>();


        CreateMap<Trabajador, TrabajadoresListarDTO>().ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Persona.CarnetIdentidad)).ForMember(dest => dest.Apellidos, opt => opt.MapFrom(src => src.Persona.ApellidoPaterno + " " + src.Persona.ApellidoMaterno)).ForMember(dest => dest.Nombres, opt => opt.MapFrom(src => src.Persona.PrimerNombre + " " + src.Persona.SegundoNombre)).ForMember(dest => dest.IdNacionalidad, opt => opt.MapFrom(src => src.Persona.IdNacionalidad)).ForMember(dest => dest.Nacionalidad, opt => opt.MapFrom(src => src.Persona.Nacionalidad.ValorCategoria)).ForMember(dest => dest.Genero, opt => opt.MapFrom(src => src.Persona.Genero)).ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Cargo.NombreCargo)).ForMember(dest => dest.NombreOficina, opt => opt.MapFrom(src => src.Cargo.Oficina.Nombre)).ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstadoTrabajador)).ForMember(dest => dest.Horarios, opt => opt.MapFrom(src => src.Horarios));

        CreateMap<Asistencia, AsistenciaListaDTO>().ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Trabajador.Persona.CarnetIdentidad)).ForMember(dest => dest.ApellidosNombres, opt => opt.MapFrom(src => src.Trabajador.Persona.ApellidoPaterno + " " + src.Trabajador.Persona.ApellidoMaterno + " " + src.Trabajador.Persona.PrimerNombre)).ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Trabajador.Cargo.NombreCargo)).ForMember(dest => dest.Oficina, opt => opt.MapFrom(src => src.Trabajador.Cargo.Oficina.Nombre)).ForMember(dest => dest.EsEntrada, opt => opt.MapFrom(src => src.EsEntrada));

        CreateMap<Falta, ListarFaltasDTO>()
            .ForMember(dest => dest.CI, opt => opt.MapFrom(src => src.Trabajador.Persona.CarnetIdentidad))
            .ForMember(dest => dest.ApellidosNombres, opt => opt.MapFrom(src => src.Trabajador.Persona.ApellidoPaterno + " " + src.Trabajador.Persona.ApellidoMaterno + " " + src.Trabajador.Persona.PrimerNombre))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.TipoFalta.ValorCategoria))
            .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.Fecha))
            .ForMember(dest => dest.Descripcion, opt => opt.MapFrom(src => src.Descripcion))
            .ForMember(dest => dest.TieneArchivoJustificativo, opt => opt.MapFrom(src => src.ArchivoJustificativo != null && src.ArchivoJustificativo.Length > 0));

        CreateMap<LogAcceso, LogsAccesoDTO>().ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario.NombreUsuario)).ForMember(dest => dest.ApellidosNombres, opt => opt.MapFrom(src => src.Usuario.Persona.ApellidoPaterno + " " + src.Usuario.Persona.ApellidoMaterno + " " + src.Usuario.Persona.SegundoNombre + " " + src.Usuario.Persona.PrimerNombre));

        CreateMap<Rol, RolDTO>();
        CreateMap<Cargo, CargoDTO>();

        // SECCION DE HISTORICOS
        CreateMap<HistoricoFalta, HistoricoFaltaListarDTO>()
            .ForMember(dest => dest.UsuarioModifico, opt => opt.MapFrom(src => src.UsuarioModifico.NombreUsuario));

        CreateMap<HistoricoUsuario, HistoricoUsuarioListarDTO>()
            .ForMember(dest => dest.UsuarioModifico, opt => opt.MapFrom(src => src.UsuarioModifico.NombreUsuario));

        CreateMap<HistoricoPersona, HistoricoPersonaListarDTO>()
        .ForMember(dest => dest.UsuarioModifico, opt => opt.MapFrom(src => src.UsuarioModifico.NombreUsuario));

        CreateMap<HistoricoTrabajador, HistoricoTrabajadorListarDTO>()
            .ForMember(dest => dest.UsuarioModifico, opt => opt.MapFrom(src => src.UsuarioModifico.NombreUsuario));
    }
}
