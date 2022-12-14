USE [BD_Estilos]
GO
/****** Object:  StoredProcedure [dbo].[BusquedaGestionesYContactos]    Script Date: 11/9/2022 7:54:32 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
-- exec BusquedaGestionesYContactos 'estilos', '', '', '51999585188' ,'','agenteprueba'
ALTER PROCEDURE [dbo].[BusquedaGestionesYContactos] (	@pVirtualCC Varchar(100) = '', @pContactId Varchar(100) = '', @pContactName Varchar(100) = '', @pContactAddress Varchar(100) = '' ,@CampaignId Varchar(100) = '', @AgentId Varchar(100) = '')
AS

Set Nocount On

Declare @QueryLimit Integer,
		@ContactImportVCC Integer

/* Limite de registros que devuelve la consulta */
Select @QueryLimit = 100

Select @ContactImportVCC = Id
From MMProDat..ContactImportVCC (Nolock)
Where Name = @pVirtualCC

Select @pContactAddress = Replace(Replace(Replace(@pContactAddress, '-', ''), '[', ''), ']', '')

/* Consulto los contactos gestionados para los que aplique el filtro */
Select Top (@QueryLimit)
	isNull(VCC, '') AS 'VCC',
	isNull(CampaignId, '') AS 'CampaignId',
	isNull(ContactId, '') AS 'ContactId',
	isNull(Identificacion, '') AS 'Identificacion',
	isNull(Nombre, ContactName) AS 'Nombre',
	isNull(Apellido1, '') AS 'Apellido',
	isNull(LastAgent, '') AS 'LastAgent',
	isNull(ManagementResultDescription, '') AS 'ManagementResultDescription',
	isNull(ContactAddress, '') AS 'ContactAddress',
	isNull(ContactAddress, '') AS 'Telefono1',
	isNull(DireccionAgenda, '') AS 'Telefono2',
	isNull(TmStmp, '1900-01-01 00:00:00.000') AS 'TmStmp'
FROM 
	Gestiones WITH(NOLOCK)
WHERE 
	VCC = @pVirtualCC
	And 
		(
			ContactId Like Case When @pContactId <> '' Then '%' + @pContactId + '%' Else ContactId End
			OR Identificacion Like Case When @pContactId <> '' Then '%' + @pContactId + '%' Else Identificacion End
		)
	And ContactName Like Case When @pContactName <> '' Then '%' + @pContactName + '%' Else ContactName End
	And IsNull(ContactAddress, '') Like Case When @pContactAddress <> '' Then '%' + @pContactAddress + '%' Else IsNull(ContactAddress, '') End
	And ManagementResultCode = '59'
	--and LastAgent = @AgentId
	--And CampaignId  =  @CampaignId 



Set Nocount Off





