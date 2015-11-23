using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using MultiTenantSurveyApp.DAL.DataModels;

namespace MultiTenantSurveyApp.DAL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.ContributorRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("EmailAddress")
                        .IsRequired();

                    b.Property<int>("SurveyId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.Question", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PossibleAnswers");

                    b.Property<int>("SurveyId");

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<int>("Type");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.Survey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("OwnerId");

                    b.Property<bool>("Published");

                    b.Property<string>("TenantId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.SurveyContributor", b =>
                {
                    b.Property<int>("SurveyId");

                    b.Property<int>("UserId");

                    b.HasKey("SurveyId", "UserId");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired();

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("IssuerValue")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 1000);

                    b.HasKey("Id");

                    b.HasIndex("IssuerValue")
                        .HasAnnotation("Relational:Name", "IssuerValueIndex");

                    b.HasAnnotation("Relational:TableName", "Tenant");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired();

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("ObjectId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 38);

                    b.Property<int>("TenantId");

                    b.HasKey("Id");

                    b.HasIndex("ObjectId")
                        .HasAnnotation("Relational:Name", "UserObjectIdIndex");

                    b.HasAnnotation("Relational:TableName", "User");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.Question", b =>
                {
                    b.HasOne("MultiTenantSurveyApp.DAL.DataModels.Survey")
                        .WithMany()
                        .HasForeignKey("SurveyId");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.Survey", b =>
                {
                    b.HasOne("MultiTenantSurveyApp.DAL.DataModels.User")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.SurveyContributor", b =>
                {
                    b.HasOne("MultiTenantSurveyApp.DAL.DataModels.Survey")
                        .WithMany()
                        .HasForeignKey("SurveyId");

                    b.HasOne("MultiTenantSurveyApp.DAL.DataModels.User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("MultiTenantSurveyApp.DAL.DataModels.User", b =>
                {
                    b.HasOne("MultiTenantSurveyApp.DAL.DataModels.Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId");
                });
        }
    }
}
