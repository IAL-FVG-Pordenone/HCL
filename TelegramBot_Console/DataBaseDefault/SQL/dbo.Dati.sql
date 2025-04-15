-- | ANDRII BALAKHTIN: Ultima modifica: 2025-04-13 in 12:01 PM  | --

CREATE TABLE [dbo].[Dati] (
    [Id]            INT IDENTITY (1, 1) NOT NULL,
    [UtenteID]      INT NOT NULL,
    [LedCucina]     BIT NULL,
    [LedSala]       BIT NULL,
    [LedBagno]      BIT NULL,
    [LedCamera]     BIT NULL,
    [AllarmeAttivo] BIT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    FOREIGN KEY ([UtenteID]) REFERENCES [dbo].[Utenti] ([Id]) ON DELETE CASCADE
);