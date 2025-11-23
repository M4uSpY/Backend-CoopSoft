using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BackendCoopSoft.DTOs.Licencias;
using BackendCoopSoft.Models;

namespace BackendCoopSoft.Services;

public static class LicenciaHelper
{
    public static decimal CalcularCantidadJornadas(
        LicenciaCrearDTO dto,
        List<Horario> horariosTrabajador)
    {
        if (horariosTrabajador == null || !horariosTrabajador.Any())
            return 0m;

        var totalJornadas = 0m;
        var fechaActual = dto.FechaInicio.Date;
        var fechaFin = dto.FechaFin.Date;
        var cultura = new CultureInfo("es-ES");

        while (fechaActual <= fechaFin)
        {
            string diaSemana = fechaActual.ToString("dddd", cultura);
            diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1); // "Lunes", "Martes", etc.

            var horario = horariosTrabajador
                .FirstOrDefault(h => h.DiaSemana == diaSemana);

            if (horario != null)
            {
                var horaEntrada = horario.HoraEntrada;
                var horaSalida = horario.HoraSalida;

                TimeSpan inicioDia;
                TimeSpan finDia;

                if (fechaActual == dto.FechaInicio.Date)
                    inicioDia = dto.HoraInicio;
                else
                    inicioDia = horaEntrada;

                if (fechaActual == dto.FechaFin.Date)
                    finDia = dto.HoraFin;
                else
                    finDia = horaSalida;

                // recortar al rango de horario
                if (inicioDia < horaEntrada) inicioDia = horaEntrada;
                if (finDia > horaSalida) finDia = horaSalida;

                if (finDia > inicioDia)
                {
                    var minutosDia = (decimal)(finDia - inicioDia).TotalMinutes;
                    var minutosJornada = (decimal)(horaSalida - horaEntrada).TotalMinutes;

                    if (minutosJornada > 0)
                    {
                        var fraccion = minutosDia / minutosJornada; // ej: 0.5, 1, 1.3

                        // redondear a pasos de 0.5 (media jornada)
                        var fraccionRedondeada =
                            Math.Round(fraccion * 2, MidpointRounding.AwayFromZero) / 2m;

                        if (fraccionRedondeada >= 0.25m)
                            totalJornadas += fraccionRedondeada;
                    }
                }
            }

            fechaActual = fechaActual.AddDays(1);
        }

        return totalJornadas;
    }
}
