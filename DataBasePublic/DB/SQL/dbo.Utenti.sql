-- | ANDRII BALAKHTIN: Ultima modifica: 2025-04-09 in 10:36 AM  | --

CREATE TABLE [dbo].[Utenti] (
    [Id]        INT       IDENTITY (1, 1) NOT NULL,
    [UtenteID]  CHAR (12) NULL,
    [ChatID]    CHAR (12) NULL,
    [abilitato] BIT       NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

