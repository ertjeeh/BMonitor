﻿// <auto-generated />
using System;
using BMonitor.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BMonitor.DAL.Migrations
{
    [DbContext(typeof(BMonitorContext))]
    [Migration("20220810060727_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BMonitor.DAL.Models.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Instance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Instances");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UpdateIntervalInMs")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Monitors");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.MonitorResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"), 1L, 1);

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("IntResult")
                        .HasColumnType("int");

                    b.Property<int>("MonitorId")
                        .HasColumnType("int");

                    b.Property<int>("StatusResultId")
                        .HasColumnType("int");

                    b.Property<string>("StringResult")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MonitorId");

                    b.HasIndex("StatusResultId");

                    b.ToTable("MonitorResults");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.HasKey("Id");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.StatusResult", b =>
                {
                    b.Property<int>("StatusResultId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("StatusResultId");

                    b.ToTable("StatusResults");

                    b.HasData(
                        new
                        {
                            StatusResultId = 0,
                            Name = "Succeeded"
                        },
                        new
                        {
                            StatusResultId = 1,
                            Name = "Unknown"
                        },
                        new
                        {
                            StatusResultId = -1,
                            Name = "Failed"
                        });
                });

            modelBuilder.Entity("CardInstance", b =>
                {
                    b.Property<int>("CardsId")
                        .HasColumnType("int");

                    b.Property<int>("InstancesId")
                        .HasColumnType("int");

                    b.HasKey("CardsId", "InstancesId");

                    b.HasIndex("InstancesId");

                    b.ToTable("CardInstance");
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Cards.HtmlCard", b =>
                {
                    b.HasBaseType("BMonitor.DAL.Models.Card");

                    b.Property<string>("Html")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("HtmlCards", (string)null);
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.FolderMonitor", b =>
                {
                    b.HasBaseType("BMonitor.DAL.Models.Monitor");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("FolderMonitors", (string)null);
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.HttpMonitor", b =>
                {
                    b.HasBaseType("BMonitor.DAL.Models.Monitor");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ExpectedStatusCode")
                        .HasColumnType("int");

                    b.Property<int>("TimeoutInMs")
                        .HasColumnType("int");

                    b.ToTable("HttpMonitors", (string)null);
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.PingMonitor", b =>
                {
                    b.HasBaseType("BMonitor.DAL.Models.Monitor");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TimeoutInMs")
                        .HasColumnType("int");

                    b.ToTable("PingMonitors", (string)null);
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.SqlMonitor", b =>
                {
                    b.HasBaseType("BMonitor.DAL.Models.Monitor");

                    b.Property<string>("ConnectionString")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Query")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.ToTable("SqlMonitors", (string)null);
                });

            modelBuilder.Entity("BMonitor.DAL.Models.MonitorResult", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Monitor", "Monitor")
                        .WithMany("MonitorResults")
                        .HasForeignKey("MonitorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BMonitor.DAL.Models.StatusResult", "StatusResult")
                        .WithMany()
                        .HasForeignKey("StatusResultId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Monitor");

                    b.Navigation("StatusResult");
                });

            modelBuilder.Entity("CardInstance", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Card", null)
                        .WithMany()
                        .HasForeignKey("CardsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BMonitor.DAL.Models.Instance", null)
                        .WithMany()
                        .HasForeignKey("InstancesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Cards.HtmlCard", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Card", null)
                        .WithOne()
                        .HasForeignKey("BMonitor.DAL.Models.Cards.HtmlCard", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.FolderMonitor", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Monitor", null)
                        .WithOne()
                        .HasForeignKey("BMonitor.DAL.Models.Monitors.FolderMonitor", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.HttpMonitor", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Monitor", null)
                        .WithOne()
                        .HasForeignKey("BMonitor.DAL.Models.Monitors.HttpMonitor", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.PingMonitor", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Monitor", null)
                        .WithOne()
                        .HasForeignKey("BMonitor.DAL.Models.Monitors.PingMonitor", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitors.SqlMonitor", b =>
                {
                    b.HasOne("BMonitor.DAL.Models.Monitor", null)
                        .WithOne()
                        .HasForeignKey("BMonitor.DAL.Models.Monitors.SqlMonitor", "Id")
                        .OnDelete(DeleteBehavior.ClientCascade)
                        .IsRequired();
                });

            modelBuilder.Entity("BMonitor.DAL.Models.Monitor", b =>
                {
                    b.Navigation("MonitorResults");
                });
#pragma warning restore 612, 618
        }
    }
}
