﻿// <auto-generated />
using System;
using Espeon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Espeon.Migrations
{
    [DbContext(typeof(EspeonDbContext))]
    [Migration("20200413121322_reminders-initial")]
    partial class remindersinitial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "5.0.0-preview.2.20120.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Espeon.GuildPrefixes", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string[]>("Values")
                        .IsRequired()
                        .HasColumnName("prefixes")
                        .HasColumnType("text[]");

                    b.HasKey("GuildId");

                    b.HasIndex("GuildId")
                        .IsUnique();

                    b.ToTable("prefixes");
                });

            modelBuilder.Entity("Espeon.UserLocalisation", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnName("guild_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Value")
                        .HasColumnName("localisation")
                        .HasColumnType("integer");

                    b.HasKey("GuildId", "UserId");

                    b.ToTable("localisation");
                });

            modelBuilder.Entity("Espeon.UserReminder", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<decimal>("ChannelId")
                        .HasColumnName("channel_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ReminderMessageId")
                        .HasColumnName("message_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<long>("TriggerAt")
                        .HasColumnName("trigger_at")
                        .HasColumnType("bigint");

                    b.Property<decimal>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnName("reminder_string")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("reminders");
                });
#pragma warning restore 612, 618
        }
    }
}
