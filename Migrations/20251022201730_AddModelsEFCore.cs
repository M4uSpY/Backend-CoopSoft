using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendCoopSoft.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsEFCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clasificador",
                columns: table => new
                {
                    id_clasificador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    valor_categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clasificador", x => x.id_clasificador);
                });

            migrationBuilder.CreateTable(
                name: "Oficina",
                columns: table => new
                {
                    id_oficina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telefono = table.Column<string>(type: "varchar(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oficina", x => x.id_oficina);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "Concepto",
                columns: table => new
                {
                    id_concepto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_concepto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tipo_concepto = table.Column<bool>(type: "bit", nullable: false),
                    codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    id_naturaleza = table.Column<int>(type: "int", nullable: false),
                    id_metodo_calculo = table.Column<int>(type: "int", nullable: false),
                    porcentaje = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    aplica_mensual = table.Column<bool>(type: "bit", nullable: false),
                    aplica_aportes = table.Column<bool>(type: "bit", nullable: false),
                    aplica_aguinaldo = table.Column<bool>(type: "bit", nullable: false),
                    aplica_prima = table.Column<bool>(type: "bit", nullable: false),
                    orden_calculo = table.Column<int>(type: "int", nullable: false),
                    es_visible = table.Column<bool>(type: "bit", nullable: false),
                    es_editable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepto", x => x.id_concepto);
                    table.ForeignKey(
                        name: "FK_Concepto_Clasificador_id_metodo_calculo",
                        column: x => x.id_metodo_calculo,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Concepto_Clasificador_id_naturaleza",
                        column: x => x.id_naturaleza,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    id_persona = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_nacionalidad = table.Column<int>(type: "int", nullable: false),
                    primer_nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    segundo_nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apellido_paterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apellido_materno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    carnet_identidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "date", nullable: false),
                    genero = table.Column<bool>(type: "bit", nullable: false),
                    direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    foto = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.id_persona);
                    table.ForeignKey(
                        name: "FK_Persona_Clasificador_id_nacionalidad",
                        column: x => x.id_nacionalidad,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Planilla",
                columns: table => new
                {
                    id_planilla = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_tipo_planilla = table.Column<int>(type: "int", nullable: false),
                    gestion = table.Column<int>(type: "int", nullable: false),
                    mes = table.Column<int>(type: "int", nullable: false),
                    periodo_desde = table.Column<DateTime>(type: "date", nullable: false),
                    periodo_hasta = table.Column<DateTime>(type: "date", nullable: false),
                    esta_cerrada = table.Column<bool>(type: "bit", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planilla", x => x.id_planilla);
                    table.ForeignKey(
                        name: "FK_Planilla_Clasificador_id_tipo_planilla",
                        column: x => x.id_tipo_planilla,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cargo",
                columns: table => new
                {
                    id_cargo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_oficina = table.Column<int>(type: "int", nullable: false),
                    nombre_cargo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargo", x => x.id_cargo);
                    table.ForeignKey(
                        name: "FK_Cargo_Oficina_id_oficina",
                        column: x => x.id_oficina,
                        principalTable: "Oficina",
                        principalColumn: "id_oficina",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Huella_Dactilar",
                columns: table => new
                {
                    id_huella = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    huella = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Huella_Dactilar", x => x.id_huella);
                    table.ForeignKey(
                        name: "FK_Huella_Dactilar_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    id_rol = table.Column<int>(type: "int", nullable: false),
                    estado_usuario = table.Column<bool>(type: "bit", nullable: false),
                    nombre_usuario = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "varchar(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_id_rol",
                        column: x => x.id_rol,
                        principalTable: "Rol",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trabajador",
                columns: table => new
                {
                    id_trabajador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    id_cargo = table.Column<int>(type: "int", nullable: false),
                    estado_trabajador = table.Column<bool>(type: "bit", nullable: false),
                    haber_basico = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajador", x => x.id_trabajador);
                    table.ForeignKey(
                        name: "FK_Trabajador_Cargo_id_cargo",
                        column: x => x.id_cargo,
                        principalTable: "Cargo",
                        principalColumn: "id_cargo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trabajador_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Persona",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_persona = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Persona", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Persona_Persona_id_persona",
                        column: x => x.id_persona,
                        principalTable: "Persona",
                        principalColumn: "id_persona",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Persona_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Planilla",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_planilla = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Planilla", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Planilla_Planilla_id_planilla",
                        column: x => x.id_planilla,
                        principalTable: "Planilla",
                        principalColumn: "id_planilla",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Planilla_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Usuario",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Usuario", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Usuario_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Historico_Usuario_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Log_Acceso",
                columns: table => new
                {
                    id_log = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    fecha_login = table.Column<DateTime>(type: "date", nullable: false),
                    hora_login = table.Column<TimeSpan>(type: "time", nullable: false),
                    fecha_logout = table.Column<DateTime>(type: "date", nullable: true),
                    hora_logout = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log_Acceso", x => x.id_log);
                    table.ForeignKey(
                        name: "FK_Log_Acceso_Usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencia",
                columns: table => new
                {
                    id_asistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "date", nullable: false),
                    hora = table.Column<TimeSpan>(type: "time", nullable: false),
                    es_entrada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencia", x => x.id_asistencia);
                    table.ForeignKey(
                        name: "FK_Asistencia_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Capacitacion",
                columns: table => new
                {
                    id_capacitacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    institucion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fecha = table.Column<DateTime>(type: "date", nullable: false),
                    carga_horaria = table.Column<int>(type: "int", nullable: false),
                    archivo_certificado = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capacitacion", x => x.id_capacitacion);
                    table.ForeignKey(
                        name: "FK_Capacitacion_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contrato",
                columns: table => new
                {
                    id_contrato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_tipo_contrato = table.Column<int>(type: "int", nullable: false),
                    id_periodo_pago = table.Column<int>(type: "int", nullable: false),
                    numero_contrato = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: false),
                    archivo_pdf = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contrato", x => x.id_contrato);
                    table.ForeignKey(
                        name: "FK_Contrato_Clasificador_id_periodo_pago",
                        column: x => x.id_periodo_pago,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contrato_Clasificador_id_tipo_contrato",
                        column: x => x.id_tipo_contrato,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contrato_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Falta",
                columns: table => new
                {
                    id_falta = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_tipo_falta = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    archivo_justificativo = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Falta", x => x.id_falta);
                    table.ForeignKey(
                        name: "FK_Falta_Clasificador_id_tipo_falta",
                        column: x => x.id_tipo_falta,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Falta_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Formacion_Academica",
                columns: table => new
                {
                    id_formacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    nivel_estudios = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    titulo_obtenido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    institucion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    anio_graduacion = table.Column<int>(type: "int", nullable: false),
                    nro_registro_profesional = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Formacion_Academica", x => x.id_formacion);
                    table.ForeignKey(
                        name: "FK_Formacion_Academica_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Trabajador",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Trabajador", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Trabajador_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Trabajador_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Horario",
                columns: table => new
                {
                    id_horario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_dia_semana = table.Column<int>(type: "int", nullable: false),
                    hora_entrada = table.Column<TimeSpan>(type: "time", nullable: false),
                    hora_salida = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Horario", x => x.id_horario);
                    table.ForeignKey(
                        name: "FK_Horario_Clasificador_id_dia_semana",
                        column: x => x.id_dia_semana,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Horario_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Solicitud",
                columns: table => new
                {
                    id_solicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_tipo_solicitud = table.Column<int>(type: "int", nullable: false),
                    id_estado_solicitud = table.Column<int>(type: "int", nullable: false),
                    motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "date", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    fecha_solicitud = table.Column<DateTime>(type: "date", nullable: false),
                    fecha_aprobacion = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitud", x => x.id_solicitud);
                    table.ForeignKey(
                        name: "FK_Solicitud_Clasificador_id_estado_solicitud",
                        column: x => x.id_estado_solicitud,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Clasificador_id_tipo_solicitud",
                        column: x => x.id_tipo_solicitud,
                        principalTable: "Clasificador",
                        principalColumn: "id_clasificador",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trabajador_Planilla",
                columns: table => new
                {
                    id_trabajador_planilla = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador = table.Column<int>(type: "int", nullable: false),
                    id_planilla = table.Column<int>(type: "int", nullable: false),
                    es_aportante = table.Column<bool>(type: "bit", nullable: false),
                    afiliado_gestora = table.Column<bool>(type: "bit", nullable: false),
                    afiliado_caja = table.Column<bool>(type: "bit", nullable: false),
                    afiliado_provivenda = table.Column<bool>(type: "bit", nullable: false),
                    nombre_cargo_mes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    haber_basico_mes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    dias_trabajados = table.Column<int>(type: "int", nullable: false),
                    horas_trabajadas = table.Column<int>(type: "int", nullable: false),
                    antiguedad_meses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajador_Planilla", x => x.id_trabajador_planilla);
                    table.ForeignKey(
                        name: "FK_Trabajador_Planilla_Planilla_id_planilla",
                        column: x => x.id_planilla,
                        principalTable: "Planilla",
                        principalColumn: "id_planilla",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trabajador_Planilla_Trabajador_id_trabajador",
                        column: x => x.id_trabajador,
                        principalTable: "Trabajador",
                        principalColumn: "id_trabajador",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Falta",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_falta = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Falta", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Falta_Falta_id_falta",
                        column: x => x.id_falta,
                        principalTable: "Falta",
                        principalColumn: "id_falta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Falta_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historico_Trabajador_Planilla",
                columns: table => new
                {
                    id_historico = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador_planilla = table.Column<int>(type: "int", nullable: false),
                    usuario_modifico = table.Column<int>(type: "int", nullable: false),
                    fecha_modificacion = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Trabajador_Planilla", x => x.id_historico);
                    table.ForeignKey(
                        name: "FK_Historico_Trabajador_Planilla_Trabajador_Planilla_id_trabajador_planilla",
                        column: x => x.id_trabajador_planilla,
                        principalTable: "Trabajador_Planilla",
                        principalColumn: "id_trabajador_planilla",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Trabajador_Planilla_Usuario_usuario_modifico",
                        column: x => x.usuario_modifico,
                        principalTable: "Usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trabajador_Planilla_Valor",
                columns: table => new
                {
                    id_trabajador_planilla_valor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_trabajador_planilla = table.Column<int>(type: "int", nullable: false),
                    id_concepto = table.Column<int>(type: "int", nullable: false),
                    valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    es_manual = table.Column<bool>(type: "bit", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(300)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajador_Planilla_Valor", x => x.id_trabajador_planilla_valor);
                    table.ForeignKey(
                        name: "FK_Trabajador_Planilla_Valor_Concepto_id_concepto",
                        column: x => x.id_concepto,
                        principalTable: "Concepto",
                        principalColumn: "id_concepto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Trabajador_Planilla_Valor_Trabajador_Planilla_id_trabajador_planilla",
                        column: x => x.id_trabajador_planilla,
                        principalTable: "Trabajador_Planilla",
                        principalColumn: "id_trabajador_planilla",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencia_id_trabajador",
                table: "Asistencia",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Capacitacion_id_trabajador",
                table: "Capacitacion",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Cargo_id_oficina",
                table: "Cargo",
                column: "id_oficina");

            migrationBuilder.CreateIndex(
                name: "IX_Clasificador_categoria_valor_categoria",
                table: "Clasificador",
                columns: new[] { "categoria", "valor_categoria" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concepto_codigo",
                table: "Concepto",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concepto_id_metodo_calculo",
                table: "Concepto",
                column: "id_metodo_calculo");

            migrationBuilder.CreateIndex(
                name: "IX_Concepto_id_naturaleza",
                table: "Concepto",
                column: "id_naturaleza");

            migrationBuilder.CreateIndex(
                name: "IX_Concepto_orden_calculo",
                table: "Concepto",
                column: "orden_calculo");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_id_periodo_pago",
                table: "Contrato",
                column: "id_periodo_pago");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_id_tipo_contrato",
                table: "Contrato",
                column: "id_tipo_contrato");

            migrationBuilder.CreateIndex(
                name: "IX_Contrato_id_trabajador",
                table: "Contrato",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Falta_id_tipo_falta",
                table: "Falta",
                column: "id_tipo_falta");

            migrationBuilder.CreateIndex(
                name: "IX_Falta_id_trabajador",
                table: "Falta",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Formacion_Academica_id_trabajador",
                table: "Formacion_Academica",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Falta_id_falta",
                table: "Historico_Falta",
                column: "id_falta");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Falta_usuario_modifico",
                table: "Historico_Falta",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Persona_id_persona",
                table: "Historico_Persona",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Persona_usuario_modifico",
                table: "Historico_Persona",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Planilla_id_planilla",
                table: "Historico_Planilla",
                column: "id_planilla");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Planilla_usuario_modifico",
                table: "Historico_Planilla",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Trabajador_id_trabajador",
                table: "Historico_Trabajador",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Trabajador_usuario_modifico",
                table: "Historico_Trabajador",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Trabajador_Planilla_id_trabajador_planilla",
                table: "Historico_Trabajador_Planilla",
                column: "id_trabajador_planilla");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Trabajador_Planilla_usuario_modifico",
                table: "Historico_Trabajador_Planilla",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Usuario_id_usuario",
                table: "Historico_Usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Usuario_usuario_modifico",
                table: "Historico_Usuario",
                column: "usuario_modifico");

            migrationBuilder.CreateIndex(
                name: "IX_Horario_id_dia_semana",
                table: "Horario",
                column: "id_dia_semana");

            migrationBuilder.CreateIndex(
                name: "IX_Horario_id_trabajador_id_dia_semana",
                table: "Horario",
                columns: new[] { "id_trabajador", "id_dia_semana" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Huella_Dactilar_id_persona",
                table: "Huella_Dactilar",
                column: "id_persona");

            migrationBuilder.CreateIndex(
                name: "IX_Log_Acceso_id_usuario",
                table: "Log_Acceso",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_id_nacionalidad",
                table: "Persona",
                column: "id_nacionalidad");

            migrationBuilder.CreateIndex(
                name: "IX_Planilla_id_tipo_planilla_gestion_mes",
                table: "Planilla",
                columns: new[] { "id_tipo_planilla", "gestion", "mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_estado_solicitud",
                table: "Solicitud",
                column: "id_estado_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_tipo_solicitud",
                table: "Solicitud",
                column: "id_tipo_solicitud");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_id_trabajador",
                table: "Solicitud",
                column: "id_trabajador");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_id_cargo",
                table: "Trabajador",
                column: "id_cargo");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_id_persona",
                table: "Trabajador",
                column: "id_persona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_Planilla_id_planilla",
                table: "Trabajador_Planilla",
                column: "id_planilla");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_Planilla_id_trabajador_id_planilla",
                table: "Trabajador_Planilla",
                columns: new[] { "id_trabajador", "id_planilla" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_Planilla_Valor_id_concepto",
                table: "Trabajador_Planilla_Valor",
                column: "id_concepto");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajador_Planilla_Valor_id_trabajador_planilla_id_concepto",
                table: "Trabajador_Planilla_Valor",
                columns: new[] { "id_trabajador_planilla", "id_concepto" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_persona",
                table: "Usuario",
                column: "id_persona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_id_rol",
                table: "Usuario",
                column: "id_rol");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_nombre_usuario",
                table: "Usuario",
                column: "nombre_usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencia");

            migrationBuilder.DropTable(
                name: "Capacitacion");

            migrationBuilder.DropTable(
                name: "Contrato");

            migrationBuilder.DropTable(
                name: "Formacion_Academica");

            migrationBuilder.DropTable(
                name: "Historico_Falta");

            migrationBuilder.DropTable(
                name: "Historico_Persona");

            migrationBuilder.DropTable(
                name: "Historico_Planilla");

            migrationBuilder.DropTable(
                name: "Historico_Trabajador");

            migrationBuilder.DropTable(
                name: "Historico_Trabajador_Planilla");

            migrationBuilder.DropTable(
                name: "Historico_Usuario");

            migrationBuilder.DropTable(
                name: "Horario");

            migrationBuilder.DropTable(
                name: "Huella_Dactilar");

            migrationBuilder.DropTable(
                name: "Log_Acceso");

            migrationBuilder.DropTable(
                name: "Solicitud");

            migrationBuilder.DropTable(
                name: "Trabajador_Planilla_Valor");

            migrationBuilder.DropTable(
                name: "Falta");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Concepto");

            migrationBuilder.DropTable(
                name: "Trabajador_Planilla");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Planilla");

            migrationBuilder.DropTable(
                name: "Trabajador");

            migrationBuilder.DropTable(
                name: "Cargo");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "Oficina");

            migrationBuilder.DropTable(
                name: "Clasificador");
        }
    }
}
