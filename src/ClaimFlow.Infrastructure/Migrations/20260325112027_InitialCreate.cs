using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;
using Pgvector;

#nullable disable

namespace ClaimFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccuredAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileData = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customers_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    PolicyStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_policies_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_policies_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "beneficiaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Relationships = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Percentage = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_beneficiaries_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    FraudRiskScore = table.Column<int>(type: "integer", nullable: true),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: true),
                    IsFraud = table.Column<bool>(type: "boolean", nullable: false),
                    SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claims_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_claims_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoverageType = table.Column<string>(type: "text", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coverages_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "premiums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    PremiumStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premiums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_premiums_policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claim_docs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_docs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_docs_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "claim_status_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ToStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claim_status_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_claim_status_history_claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_beneficiaries_PolicyId",
                table: "beneficiaries",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_docs_ClaimId",
                table: "claim_docs",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claim_status_history_ClaimId",
                table: "claim_status_history",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_ClaimNumber",
                table: "claims",
                column: "ClaimNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_claims_PolicyId",
                table: "claims",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_claims_SearchVector",
                table: "claims",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "IX_claims_TenantId",
                table: "claims",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_coverages_PolicyId",
                table: "coverages",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_customers_Email",
                table: "customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId",
                table: "customers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_ProcessedAt",
                table: "outbox_message",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_policies_CustomerId",
                table: "policies",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_policies_PolicyNumber",
                table: "policies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policies_TenantId",
                table: "policies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_premiums_PolicyId",
                table: "premiums",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_Code",
                table: "tenants",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficiaries");

            migrationBuilder.DropTable(
                name: "claim_docs");

            migrationBuilder.DropTable(
                name: "claim_status_history");

            migrationBuilder.DropTable(
                name: "coverages");

            migrationBuilder.DropTable(
                name: "outbox_message");

            migrationBuilder.DropTable(
                name: "premiums");

            migrationBuilder.DropTable(
                name: "claims");

            migrationBuilder.DropTable(
                name: "policies");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
