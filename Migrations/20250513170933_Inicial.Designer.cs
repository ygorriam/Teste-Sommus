﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sommus.DengueApi.Data;

#nullable disable

namespace Sommus.DengueApi.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250513170933_Inicial")]
    partial class Inicial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Sommus.DengueApi.Models.DengueDados", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CasosEstimados")
                        .HasColumnType("int");

                    b.Property<int>("CasosNotificados")
                        .HasColumnType("int");

                    b.Property<int>("NivelAlerta")
                        .HasColumnType("int");

                    b.Property<string>("SemanaEpidemiologica")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("DengueDados");
                });
#pragma warning restore 612, 618
        }
    }
}
