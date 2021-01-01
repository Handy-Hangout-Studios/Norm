﻿// <auto-generated />
using Harold.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Harold.Migrations
{
    [DbContext(typeof(BotPsqlContext))]
    partial class BotPsqlContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityByDefaultColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("Harold.Database.Entities.GuildNovelRegistration", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("NovelInfoId")
                        .HasColumnType("integer")
                        .HasColumnName("novel_info_id");

                    b.Property<decimal>("AnnouncementChannelId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("announcement_channel_id");

                    b.Property<bool>("PingEveryone")
                        .HasColumnType("boolean")
                        .HasColumnName("ping_everyone");

                    b.Property<bool>("PingNoOne")
                        .HasColumnType("boolean")
                        .HasColumnName("ping_no_one");

                    b.Property<decimal?>("RoleId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("role_id");

                    b.HasKey("GuildId", "NovelInfoId", "AnnouncementChannelId");

                    b.HasIndex("NovelInfoId");

                    b.ToTable("guild_novel_registration");
                });

            modelBuilder.Entity("Harold.Database.Entities.NovelInfo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .UseIdentityByDefaultColumn();

                    b.Property<decimal>("FictionId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("fiction_id");

                    b.Property<string>("FictionUri")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("fiction_uri");

                    b.Property<decimal>("MostRecentChapterId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("most_recent_chapter_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("novel_name");

                    b.Property<string>("SyndicationUri")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("syndication_uri");

                    b.HasKey("Id")
                        .HasName("id");

                    b.ToTable("novel_info");
                });

            modelBuilder.Entity("Harold.Database.Entities.GuildNovelRegistration", b =>
                {
                    b.HasOne("Harold.Database.Entities.NovelInfo", "NovelInfo")
                        .WithMany("AssociatedGuildNovelRegistrations")
                        .HasForeignKey("NovelInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NovelInfo");
                });

            modelBuilder.Entity("Harold.Database.Entities.NovelInfo", b =>
                {
                    b.Navigation("AssociatedGuildNovelRegistrations");
                });
#pragma warning restore 612, 618
        }
    }
}
