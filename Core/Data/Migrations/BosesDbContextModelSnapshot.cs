using BosesApp.Core.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BosesApp.Core.Data.Migrations
{
    [DbContext(typeof(BosesDbContext))]
    partial class BosesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("BosesApp.Core.Data.Models.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AccessibilitySettings")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessibilityNeeds")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<DateTime?>("DateOfBirth")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GuardianName")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("GuardianPhoneNumber")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<bool>("HasCompletedOnboarding")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVoiceAuthEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LinkedBankAccounts")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastLoginAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now')");

                    b.Property<int>("PreferredLanguage")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PwdCategory")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PwdIdNumber")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("SeniorCitizenIdNumber")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("UserType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VoicePrintData")
                        .HasColumnType("TEXT");

                    b.Property<bool>("VoiceOnlyMode")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Email");

                    b.HasIndex("PhoneNumber")
                        .IsUnique();

                    b.ToTable("UserProfiles");
                });
#pragma warning restore 612, 618
        }
    }
}
