using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VerificarApi.Domain.Entities;

namespace VerificarApi.Infrastructure.Data.Configurations
{
    public class VehicleAnalysisConfiguration : IEntityTypeConfiguration<VehicleAnalysis>
    {
        public void Configure(EntityTypeBuilder<VehicleAnalysis> builder)
        {
            builder.ToTable("VehicleAnalyses");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Marca).IsRequired().HasMaxLength(100);
            builder.Property(a => a.Modelo).IsRequired().HasMaxLength(300);
            builder.Property(a => a.AnoModelo).IsRequired();

            // Textos longos gerados pela IA
            builder.Property(a => a.ProblemasCronicos).HasColumnType("longtext");
            builder.Property(a => a.PontosPositivos).HasColumnType("longtext");
            builder.Property(a => a.PontosNegativos).HasColumnType("longtext");
            builder.Property(a => a.ResumoGeral).HasColumnType("longtext");

            // Notas
            builder.Property(a => a.NotaConfiabilidade).HasColumnType("decimal(3,2)");
            builder.Property(a => a.NotaCustoManutencao).HasColumnType("decimal(3,2)");
            builder.Property(a => a.NotaConforto).HasColumnType("decimal(3,2)");
            builder.Property(a => a.NotaDesempenho).HasColumnType("decimal(3,2)");
            builder.Property(a => a.NotaConsumo).HasColumnType("decimal(3,2)");
            builder.Property(a => a.NotaEspacoInterno).HasColumnType("decimal(3,2)");

            builder.Property(a => a.ModeloIA).HasMaxLength(50);

            // Índice para busca rápida por marca+modelo+ano
            builder.HasIndex(a => new { a.Marca, a.Modelo, a.AnoModelo });
        }
    }
}
