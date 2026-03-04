namespace Aigen.Core.Metadata;

/// <summary>
/// Clasificación funcional de una tabla de BD.
/// Determina qué operaciones CRUD y qué plantillas genera AIGEN.
///
/// Prefijos Incoder:    TM_ TB_ TBR_ TP_ TR_ TC_ TA_ TS_ TH_ TI_ TX_
/// Sin prefijo:         Se clasifica por heurística de nombre y estructura
/// </summary>
public enum TableType
{
    // ── Con prefijo Incoder ───────────────────────────────────
    Movement,       // TM_  — Transaccional. CRUD completo + paginación
    Basic,          // TB_  — Catálogo simple. CRUD completo
    BasicRelated,   // TBR_ — Catálogo con FK padre. CRUD completo
    Parameter,      // TP_  — Parametrización compleja. CRUD + Toggle Estado
    Relational,     // TR_  — N:M pura. Insert/Delete/Select
    Composition,    // TC_  — Hijo que muere con el padre. CRUD completo
    Audit,          // TA_  — Auditoría. Solo lectura
    System,         // TS_  — Sistema global. Solo lectura en UI
    Historical,     // TH_  — Histórico/Snapshot. Solo lectura
    Image,          // TI_  — Binarios 1:1. Gestión binarios
    Dictionary,     // TX_  — Búsqueda fulltext. Solo lectura

    // ── Sin prefijo (clasificado por heurística) ──────────────
    Unknown         // Fallback → CRUD completo igual que Movement
}
