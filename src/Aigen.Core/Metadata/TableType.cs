namespace Aigen.Core.Metadata;

/// <summary>
/// Tipo de tabla segun su prefijo (estandar Incoder/AIGEN).
/// Determina que operaciones CRUD se generan por cada tabla.
/// </summary>
public enum TableType
{
    Movement,       // TM_ - Transaccional. CRUD completo.
    Basic,          // TB_ - Sin FK.        CRUD completo.
    BasicRelated,   // TBR_ - Con padre FK. CRUD completo.
    Parameter,      // TP_ - Compleja.      CRUD + Estado.
    Relational,     // TR_ - N:M sin attrs. Insert/Delete/Select.
    Audit,          // TA_ - Auditoria.     Solo lectura.
    System,         // TS_ - Global.        Solo lectura UI.
    Composition,    // TC_ - Padre-hijo.    CRUD completo.
    Dictionary,     // TX_ - Fulltext.      Solo lectura.
    Historical,     // TH_ - Historicos.    Solo lectura.
    Image,          // TI_ - Imagenes 1:1.  Gestion binarios.
    Unknown         // Sin prefijo.         CRUD completo.
}
