using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region CONTROLE DE VERSÃO

    public string geraBlocoBeginTran()
    {
        return string.Format(@"
                BEGIN 
                        DECLARE @Erro INT
                        DECLARE @MensagemErro nvarchar(2048)
                        DECLARE @CodigoRetorno INT
                        SET @Erro = 0
                        BEGIN TRAN
                            BEGIN TRY ");
    }

    public string geraBlocoEndTran()
    {
        return string.Format(@"
		                END TRY
	                    BEGIN CATCH
		                    SET @Erro = ERROR_NUMBER()
		                    SET @MensagemErro = ERROR_MESSAGE()
	                    END CATCH

	                    IF @Erro = 0
	                    BEGIN
		                    SELECT 'OK' AS ErrorMessage;
		                    COMMIT
	                    END
	                    ELSE
	                    BEGIN
		                    SELECT @MensagemErro AS ErrorMessage;
		                    ROLLBACK
	                    END
                    END ");
    }
    public string geraBlocoEndTran_ComRetorno()
    {
        return string.Format(@"
		                END TRY
	                    BEGIN CATCH
		                    SET @Erro = ERROR_NUMBER()
		                    SET @MensagemErro = ERROR_MESSAGE()
	                    END CATCH

	                    IF @Erro = 0
	                    BEGIN
		                    SELECT 'OK' AS ErrorMessage, @CodigoRetorno as Retorno
		                    COMMIT
	                    END
	                    ELSE
	                    BEGIN
		                    SELECT @MensagemErro AS ErrorMessage, -1 as Retorno
		                    ROLLBACK
	                    END
                    END");
    }

    #endregion
}
}