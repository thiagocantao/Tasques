using BriskPtf.DataSources;
using DevExpress.Data;
using DevExpress.Data.PivotGrid;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.Web.ASPxPivotGrid;
using DevExpress.Web.ASPxTreeList;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraPivotGrid;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region Integração Zeus

    public DataSet getArquivoExportacaoZeus()
    {
        comandoSQL = string.Format(@"
                    SELECT * FROM {0}.{1}.f_GeraArquivoExportacaoZeus()
                    ORDER BY NomeUnidade,NomeProjeto,NomeAcao
            ", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGeraArquivoCronogramaOrcamentario(int codigoProjeto, string sWhere)
    {
        comandoSQL = string.Format(@"
                    select resultado.*,
(select sum(isnull(valorTotal,0)) 
      from {0}.{1}.CronogramaOrcamentario soma 
      where soma.CodigoProjeto = resultado.CodigoProjeto ) totalProjeto,                                   
(select sum(isnull(valorTotal,0)) 
      from   {0}.{1}.CronogramaOrcamentario soma inner join
            {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
      where soma.CodigoProjeto = resultado.CodigoProjeto 
      and ai.CodigoAcaoSuperior = resultado.codigoPai) totalAcao,
(select sum(isnull(valorTotal,0)) 
            from {0}.{1}.CronogramaOrcamentario soma inner join
            {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
            where soma.CodigoProjeto = resultado.CodigoProjeto 
            and soma.CodigoAcao = resultado.CodigoAcao) totalAtividade,
(select sum(isnull(valorTotal,0)) 
		 	     from {0}.{1}.CronogramaOrcamentario soma inner join
		 	          {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
		 	    where soma.CodigoProjeto = resultado.CodigoProjeto
		 	      and ai.FonteRecurso = 'FU') totalFundecop,
(select sum(isnull(valorTotal,0)) 
		 	     from {0}.{1}.CronogramaOrcamentario soma inner join
		 	          {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
		 	    where soma.CodigoProjeto = resultado.CodigoProjeto
		 	      and ai.FonteRecurso = 'RP') totalRP
from (  
            SELECT t1.CodigoProjeto,
            p.NomeProjeto,
            t1.CodigoAcao AS CodigoAcao, 
            t1.CodigoAcaoSuperior AS CodigoPai,
            CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior 
            THEN CONVERT(VARCHAR, t1.NumeroAcao)
            ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END AS Numero,
            tsup.NomeAcao NomeAcao,
            t1.NomeAcao AS NomeAtividade,
            t1.FonteRecurso,
            co.SeqPlanoContas, 
            co.Quantidade, 
            co.ValorUnitario,
            co.ValorTotal, 
            co.MemoriaCalculo, 
            co.Plan01 Janeiro, 
            co.Plan02 Fevereiro, 
            co.Plan03 Marco, 
            co.Plan04 Abril, 
            co.Plan05 Maio,
            co.Plan06 Junho, 
            co.Plan07 Julho, 
            co.Plan08 Agosto, 
            co.Plan09 Setembro, 
            co.Plan10 Outubro, 
            co.Plan11 Novembro, 
            co.Plan12 Dezembro, 
            opc.CONTA_COD +' - ' +opc.CONTA_DES ContaOrcamentaria,
            opc.CONTA_DES, 
            opc.CONTA_COD,
            tSup.NumeroAcao acaoSup,
            t1.NumeroAcao 
						  , CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4)
									ELSE right('0000'+CONVERT(VARCHAR, tSup.NumeroAcao),4) + '.' + right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4) 
							END AS ordem 
       
            FROM  {0}.{1}.tai02_AcoesIniciativa t1 LEFT JOIN
                  {0}.{1}.tai02_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior left JOIN
                  {0}.{1}.CronogramaOrcamentario co ON (co.CodigoAcao = t1.CodigoAcao) left join
                  {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas inner join
		          {0}.{1}.Projeto p on (p.CodigoProjeto = t1.CodigoProjeto)
            WHERE t1.CodigoProjeto = {2}
              and t1.IndicaSemRecurso = 'N'
) resultado        
ORDER BY ordem 
--resultado.Numero, 
--         CASE WHEN resultado.CodigoAcao = resultado.CodigoPai THEN CONVERT(VARCHAR, resultado.Numero)
--              ELSE CONVERT(VARCHAR, resultado.acaoSup) + '.' + CONVERT(VARCHAR, resultado.NumeroAcao) 
--         END
         ", bancodb, Ownerdb, codigoProjeto);
        return getDataSet(comandoSQL);
    }



    public DataSet getDadosGeraArquivoCronogramaOrcamentario()
    {
        comandoSQL = string.Format(@"
                    select resultado.*,
(select sum(isnull(valorTotal,0)) 
      from CronogramaOrcamentario soma 
      where soma.CodigoProjeto = resultado.CodigoProjeto ) totalProjeto,                                   
(select sum(isnull(valorTotal,0)) 
      from   CronogramaOrcamentario soma inner join
            tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
      where soma.CodigoProjeto = resultado.CodigoProjeto 
      and ai.CodigoAcaoSuperior = resultado.codigoPai) totalAcao,
(select sum(isnull(valorTotal,0)) 
            from CronogramaOrcamentario soma inner join
            tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
            where soma.CodigoProjeto = resultado.CodigoProjeto 
            and soma.CodigoAcao = resultado.CodigoAcao) totalAtividade,
(select sum(isnull(valorTotal,0)) 
		 	     from {0}.{1}.CronogramaOrcamentario soma inner join
		 	          {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
		 	    where soma.CodigoProjeto = resultado.CodigoProjeto
		 	      and ai.FonteRecurso = 'FU') totalFundecop,
(select sum(isnull(valorTotal,0)) 
		 	     from {0}.{1}.CronogramaOrcamentario soma inner join
		 	          {0}.{1}.tai02_AcoesIniciativa ai on (ai.CodigoProjeto = soma.CodigoProjeto and ai.CodigoAcao = soma.CodigoAcao)
		 	    where soma.CodigoProjeto = resultado.CodigoProjeto
		 	      and ai.FonteRecurso = 'RP') totalRP
from (  
            SELECT t1.CodigoProjeto,
            p.NomeProjeto,
            t1.CodigoAcao AS CodigoAcao, 
            t1.CodigoAcaoSuperior AS CodigoPai,
            CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior 
            THEN CONVERT(VARCHAR, t1.NumeroAcao)
            ELSE CONVERT(VARCHAR, tSup.NumeroAcao) + '.' + CONVERT(VARCHAR, t1.NumeroAcao) END AS Numero,
            tsup.NomeAcao NomeAcao,
            t1.NomeAcao AS NomeAtividade,
            t1.FonteRecurso,
            co.SeqPlanoContas, 
            co.Quantidade, 
            co.ValorUnitario,
            co.ValorTotal, 
            co.MemoriaCalculo, 
            co.Plan01 Janeiro, 
            co.Plan02 Fevereiro, 
            co.Plan03 Marco, 
            co.Plan04 Abril, 
            co.Plan05 Maio,
            co.Plan06 Junho, 
            co.Plan07 Julho, 
            co.Plan08 Agosto, 
            co.Plan09 Setembro, 
            co.Plan10 Outubro, 
            co.Plan11 Novembro, 
            co.Plan12 Dezembro, 
            opc.CONTA_COD +' - ' +opc.CONTA_DES ContaOrcamentaria,
            opc.CONTA_DES, 
            opc.CONTA_COD,
            tSup.NumeroAcao acaoSup,
            t1.NumeroAcao  
						  , CASE WHEN t1.CodigoAcao = t1.CodigoAcaoSuperior THEN right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4)
									ELSE right('0000'+CONVERT(VARCHAR, tSup.NumeroAcao),4) + '.' + right('0000'+CONVERT(VARCHAR, t1.NumeroAcao),4) 
							END AS ordem
       
            FROM  {0}.{1}.tai02_AcoesIniciativa t1 LEFT JOIN
                  {0}.{1}.tai02_AcoesIniciativa tSup ON tSup.CodigoAcao = t1.CodigoAcaoSuperior left JOIN
                  {0}.{1}.CronogramaOrcamentario co ON (co.CodigoAcao = t1.CodigoAcao) left join
                  {0}.{1}.orc_planoContas opc ON opc.SeqPlanoContas = co.SeqPlanoContas inner join
		          {0}.{1}.Projeto p on (p.CodigoProjeto = t1.CodigoProjeto)
            WHERE t1.IndicaSemRecurso = 'N'
) resultado        
ORDER BY ordem 
--resultado.Numero, 
--         CASE WHEN resultado.CodigoAcao = resultado.CodigoPai THEN CONVERT(VARCHAR, resultado.Numero)
--              ELSE CONVERT(VARCHAR, resultado.acaoSup) + '.' + CONVERT(VARCHAR, resultado.NumeroAcao) 
--         END
         ", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }


    public DataSet getDadosGeraRelatoriosReformulacao(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            select 
            p.CodigoProjeto, p.NomeProjeto
            from {0}.{1}.Projeto p inner join
                 {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto
            where ta.EtapaOrcamento = 1 -- somente projetos que estejam em formulação
              and ta.CodigoMovimentoOrcamento = (select CodigoMovimentoOrcamento from {0}.{1}.orc_movimentoOrcamento where ano = YEAR(getdate()) )
              and p.codigoEntidade = {2}
              and p.CodigoStatusProjeto = 3 --somente projetos que estão em execução 

        ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }


    public DataSet getDadosTerminoReformulacao(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            select 
            p.CodigoProjeto, p.NomeProjeto
            from {0}.{1}.Projeto p inner join
                 {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto
            where ta.EtapaOrcamento = 3 --somente projetos que estejam em reformulação
              and ta.CodigoMovimentoOrcamento = (select CodigoMovimentoOrcamento from {0}.{1}.orc_movimentoOrcamento where ano = YEAR(getdate()) )
            and p.codigoEntidade = {2} 
        ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosDesbloqueioReformulacao(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            select ROW_NUMBER() OVER (order by p.codigoprojeto) AS linha,  
            p.CodigoProjeto, p.NomeProjeto, ta.DataUltimoBloqueio, u.NomeUsuario , u.CodigoUsuario
            from {0}.{1}.Projeto p inner join
                 {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto inner join
                 {0}.{1}.usuario u on u.codigousuario = ta.codigoUsuarioBloqueio
            where ta.EtapaOrcamento = 3 --somente projetos que estejam em reformulação
              and ta.InBloqueioReformulacao = 'S' -- somente projetos que estejam bloqueados
            and   p.codigoEntidade = {2} 
        ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosInicioReformulacao(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            select o.dtInicioReformulacao, o.dtTerminoReformulacao, o.mesReferenciaZeus
            from {0}.{1}.orc_movimentoOrcamento o
            where o.ano = YEAR(getdate())
            and codigoEntidade = {2} 
 
        ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }


    public DataSet iniciaReformulacao(char acao, int codigousuario, int qtdProjetos, string mes, int CodigoEntidade)
    {
        if (acao == 'I')
        {
            comandoSQL = string.Format(@"
                        BEGIN
                          UPDATE {0}.{1}.orc_movimentoOrcamento
                                set [dtInicioReformulacao] = getdate()
                                   ,[dtTerminoReformulacao] = null
                                   ,[mesReferenciaZeus] = '{4}'
                                   ,[CodigoUsuarioInicio] = {2} 
                                   ,[QuantidadeProjetos] = {3}  
                          where ano = YEAR(getdate())
                            and codigoEntidade = {5}  
                       END 
       ", bancodb, Ownerdb, codigousuario, qtdProjetos, mes, CodigoEntidade);
        }
        else if (acao == 'D')
        {
            comandoSQL = string.Format(@"
                        BEGIN
                          UPDATE {0}.{1}.orc_movimentoOrcamento
                                set [dtInicioReformulacao] = null
                                   ,[dtTerminoReformulacao] = null
                                   ,[mesReferenciaZeus] = null
                                   ,[CodigoUsuarioInicio] = null
                                   ,[QuantidadeProjetos] = null
                          where ano = YEAR(getdate())
                            and codigoEntidade = {5}  
                       END 
       ", bancodb, Ownerdb, codigousuario, qtdProjetos, mes, CodigoEntidade);
        }
        return getDataSet(comandoSQL);
    }

    public DataSet limpaReformulacao(int codigousuario, int CodigoEntidade)
    {
        comandoSQL = string.Format(@"
                        BEGIN
                          UPDATE {0}.{1}.orc_movimentoOrcamento
                                set [dtInicioReformulacao] = null
                                   ,[dtTerminoReformulacao] = null
                                   ,[mesReferenciaZeus] = null
                                   ,[CodigoUsuarioInicio] = null
                                   ,[QuantidadeProjetos] = null
                          where ano = YEAR(getdate())
                            and codigoEntidade = {3}

  
                       update {0}.{1}.CronogramaOrcamentarioAcao 
                            set EtapaAcaoAtividade = 'F'
                            where CodigoProjeto in (    
                                 select 
                                    p.CodigoProjeto
                                    from {0}.{1}.Projeto p inner join
                                            {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto
                                    where ta.EtapaOrcamento <> 1 -- somente projetos que estejam em reformulação
                                        and ta.CodigoMovimentoOrcamento = (select CodigoMovimentoOrcamento from {0}.{1}.orc_movimentoOrcamento where ano = YEAR(getdate()) )
                                        and p.codigoEntidade = {3} 
                                        and exists (select 1 from CronogramaOrcamentarioAcao ca where ca.CodigoProjeto = p.CodigoProjeto)
                                        )

                      update {0}.{1}.tai02_AcoesIniciativa 
                            set EtapaAcaoAtividade = 'F'
                            where CodigoProjeto in (    
                                 select 
                                    p.CodigoProjeto
                                    from {0}.{1}.Projeto p inner join
                                            {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto
                                    where ta.EtapaOrcamento <> 1 -- somente projetos que estejam em reformulação
                                        and ta.CodigoMovimentoOrcamento = (select CodigoMovimentoOrcamento from {0}.{1}.orc_movimentoOrcamento where ano = YEAR(getdate()) )
                                        and p.codigoEntidade = {3} 
                                        and exists (select 1 from CronogramaOrcamentarioAcao ca where ca.CodigoProjeto = p.CodigoProjeto)
                                        )

                        update {0}.{1}.TermoAbertura02 
                            set EtapaOrcamento = 1,
                                InBloqueioReformulacao = 'N',
                                DataUltimoBloqueio = null,
                                DataUltimoDesbloqueio = null,
                                codigoUsuarioBloqueio = null,
                                codigoUsuarioDesbloqueio = null
                            where CodigoProjeto in (    
                                 select 
                                    p.CodigoProjeto
                                    from {0}.{1}.Projeto p inner join
                                            {0}.{1}.TermoAbertura02 ta on ta.CodigoProjeto = p.CodigoProjeto
                                    where ta.EtapaOrcamento <> 1 -- somente projetos que estejam em formulação
                                        and ta.CodigoMovimentoOrcamento = (select CodigoMovimentoOrcamento from {0}.{1}.orc_movimentoOrcamento where ano = YEAR(getdate()) )
                                        and p.codigoEntidade = {3} 
                                        and exists (select 1 from CronogramaOrcamentarioAcao ca where ca.CodigoProjeto = p.CodigoProjeto)
                                        )

                            delete from ConteudoAnexo where codigoSequencialAnexo in ( select codigoSequencialAnexo  from AnexoVersao a inner join 
                                                                                                Anexo b on a.codigoanexo = b.CodigoAnexo and b.Nome = '_______TAI_ORIGINAL.pdf')
                            delete from AnexoVersao where CodigoAnexo in ( select CodigoAnexo from Anexo where Nome = '_______TAI_ORIGINAL.pdf')
                            delete from AnexoAssociacao where CodigoAnexo in ( select CodigoAnexo from Anexo where Nome = '_______TAI_ORIGINAL.pdf')
                            delete from Anexo where Nome = '_______TAI_ORIGINAL.pdf'

                            DECLARE @CodigoFluxo    INT
                
                            select @CodigoFluxo = codigofluxo from Fluxos where CodigoEntidade = @CodigoEntidade and IniciaisFluxo = 'RFRMPRJ'
  
				            delete from [dbo].[FluxosStatusProjeto] where [CodigoFluxo] = @CodigoFluxo				
                            delete from  [dbo].[FluxosProjeto] where [CodigoFluxo] = @CodigoFluxo

                       END 
       ", bancodb, Ownerdb, codigousuario, CodigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet encerraReformulacao(int codigousuario, int codigoEntidade)
    {
        comandoSQL = string.Format(@"
        BEGIN
                DECLARE @CodigoUsuario  INT,
                        @CodigoEntidade INT,
                        @CodigoFluxo    INT

                SET @CodigoUsuario      = {2}
                SET @CodigoEntidade     = {3}
                
           select @CodigoFluxo = codigofluxo from Fluxos where CodigoEntidade = @CodigoEntidade and IniciaisFluxo = 'RFRMPRJ'

          UPDATE {0}.{1}.orc_movimentoOrcamento
                set [dtTerminoReformulacao] = getdate() ,
                    [CodigoUsuarioTermino] = {2}
          where ano = YEAR(getdate())  
            and codigoEntidade = {3} 


          UPDATE  [dbo].[FluxosProjeto]
             SET  [MaximaOcorrencia] = 0 
           WHERE  [CodigoFluxo] = @CodigoFluxo

          -- altera todos os termos de abertura para etapa 4 encerrado de todos os projetos que estavam em reformulação
          UPDATE {0}.{1}.[TermoAbertura02]
              SET [EtapaOrcamento] = 4 --altera para etapa fim da reformulação
            WHERE [EtapaOrcamento] = 3 --os projetos que estão em reformulação


        END  
        ", bancodb, Ownerdb, codigousuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet alteraEtapaReformulacao(int codigoProjeto, int etapa)
    {
        comandoSQL = string.Format(@"
           UPDATE {0}.{1}.[TermoAbertura02]
              SET [EtapaOrcamento] = {2} 
            WHERE [CodigoProjeto] = {3}
        ", bancodb, Ownerdb, etapa, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public int buscaRealizadoZeus(string mesReferencia)
    {

        int Retorno = -1;
        try
        {
            string comandoSQL = string.Format(@"
                        DECLARE @RC int
                        DECLARE @Mes int 
                        set @RC = 0
                        BEGIN TRY
	                            select @Mes = month(convert(date, '2000-'+'{2}'+'-01',103))
	                        END TRY
	                        BEGIN CATCH
	                            set @RC =  -1;
	                        END CATCH      
 
                        if (@RC != -1)
			                 EXECUTE @RC = [dbo].[p_BuscaRealizadoZeusReformulacao]  @Mes

                        SELECT @RC as RC", bancodb, Ownerdb, mesReferencia);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                Retorno = int.Parse(ds.Tables[0].Rows[0]["RC"].ToString());
        }
        catch (Exception)
        {
            Retorno = -1;
        }
        return Retorno;
    }

    public DataSet insereFluxosProjetos(int codigoProjeto, int codigoEntidade, int codigoUsuario)
    {
        comandoSQL = string.Format(@"
               BEGIN
                DECLARE @CodigoUsuario  INT,
                        @CodigoEntidade INT,
                        @CodigoFluxo    INT,
                        @CodigoProjeto  INT

                SET @CodigoUsuario      = {2}
                SET @CodigoEntidade     = {3}
                SET @CodigoProjeto      = {4}
                
                select @CodigoFluxo = codigofluxo from Fluxos where CodigoEntidade = @CodigoEntidade and IniciaisFluxo = 'RFRMPRJ'
  
								INSERT INTO [dbo].[FluxosProjeto]
													 ([CodigoFluxo]
													 ,[CodigoProjeto]
													 ,[StatusRelacionamento]
													 ,[TextoOpcaoFluxo]
													 ,[DataAtivacao]
													 ,[IdentificadorUsuarioAtivacao]
													 ,[DataDesativacao]
													 ,[IdentificadorUsuarioDesativacao]
													 ,[TipoOcorrenciaFluxo]
													 ,[MaximaOcorrencia])
										 VALUES
													 (@CodigoFluxo
													 ,@CodigoProjeto
													 ,'A'
													 ,'Reformulação'
													 ,GETDATE()
													 ,@CodigoUsuario
													 ,null
													 ,null
													 ,'N'
													 ,1)
										
										
								INSERT INTO [dbo].[FluxosStatusProjeto]
													 ([CodigoFluxo]
													 ,[CodigoProjeto]
													 ,[CodigoStatus]
													 ,[StatusRelacionamento]
													 ,[DataAtivacao]
													 ,[IdentificadorUsuarioAtivacao]
													 ,[DataDesativacao]
													 ,[IdentificadorUsuarioDesativacao])
										 VALUES
													 (@CodigoFluxo
													 ,@CodigoProjeto
													 ,3
													 ,'A'
													 ,GETDATE()
													 ,@CodigoUsuario
													 ,null
													 ,null)
               END
        ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet alteraUsuarioReformulacao(int codigoProjeto, int codigousuario)
    {

        comandoSQL = string.Format(@"
           UPDATE {0}.{1}.[TermoAbertura02]
              SET [DataUltimoBloqueio] = GETDATE(),
                  [codigoUsuarioBloqueio] = {3}
            WHERE [CodigoProjeto] = {2}
            ", bancodb, Ownerdb, codigoProjeto, codigousuario);

        return getDataSet(comandoSQL);
    }

    public DataSet incluiLogReformulacao(int codigoProjeto, string etapa, string acao, int codigousuario)
    {
        comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.[LogReformulacao]
                           ([CodigoProjeto]
                           ,[Data]
                           ,[Etapa]
                           ,[Acao]
                           ,[CodigoUsuario])
                     VALUES
                           ({3}
                           ,getdate()
                           ,'{2}'
                           ,'{4}'
                           ,{5})
        ", bancodb, Ownerdb, etapa, codigoProjeto, acao, codigousuario);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosReformulacao()
    {
        comandoSQL = string.Format(@"
            SELECT [Id] Chave
      ,[DataInicio]
      ,[DataTermino]
      ,[CodigoUsuarioInicio]
      ,u.NomeUsuario UsuarioInicio
      ,[CodigoUsuarioTermino]
      ,u2.NomeUsuario UsuarioTermino
      ,[QuantidadeProjetos]
      ,[MesReferenciaZeus]
	  ,(CASE WHEN (CodigoUsuarioTermino is null ) THEN 'S' ELSE 'N' END) AS Editavel
       FROM {0}.{1}.[Reformulacao] fr left join 
             {0}.{1}.usuario u on u.CodigoUsuario = fr.[CodigoUsuarioInicio] left join 
             {0}.{1}.usuario u2 on u2.CodigoUsuario = fr.[CodigoUsuarioTermino]
             order by fr.ID desc
        ", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    public DataSet getRelAcompanhamentoOrcamentario(int codigoEntidade)
    {
        string comandoSQL = string.Format(@"
               select pc.Ano, 
               case when pc.DespesaReceita = 'D' then 'Despesa' else 'Receita' end DespesaReceita, 
               p.NomeProjeto, ti.NomeAcao, pc.CONTA_DES, 
               coa.Quantidade, coa.ValorUnitario,
               coa.ValorProposto, coa.ValorRealizado, coa.DisponibilidadeAtual, 
               coa.ValorSuplemento, coa.ValorTransposto, coa.ValorSuplementacao_Old, coa.ValorTransposicao_Old,
               coa.DisponibilidadeReformulada
          from {0}.{1}.CronogramaOrcamentarioAcao coa inner join
               {0}.{1}.orc_planoContas pc on pc.SeqPlanoContas = coa.SeqPlanoContas inner join
               {0}.{1}.tai02_AcoesIniciativa ti on ti.CodigoAcao = coa.CodigoAcao and
                                           ti.CodigoProjeto = coa.CodigoProjeto inner join
               {0}.{1}.Projeto p on p.CodigoProjeto = coa.CodigoProjeto
         where p.codigoEntidade = {2}

            ", bancodb, Ownerdb, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGeraArquivo(int codigoEntidade, string tipo, string arquivo)
    {
        string comandoSQL = string.Format(@"
            begin 
	            DECLARE @RC int
	            DECLARE @Mes int 
	            DECLARE @Ano int 
              DECLARE @tipo char(1) 
              DECLARE @CodigoEntidade int
	          set @RC = 0
              set @tipo = '{3}'
              set @CodigoEntidade = 1
	            BEGIN TRY
			            select  @Mes = month(convert(date, '2000-'+omo.mesReferenciaZeus+'-01',103))
			                   ,@Ano = omo.ano
			            from {0}.{1}.orc_movimentoOrcamento omo
			            where omo.codigoEntidade = @CodigoEntidade
				            and omo.ano = YEAR(GETDATE())	
		            END TRY
		            BEGIN CATCH
				            RAISERROR('Não possível executar o procedimento solicitado! Não foi encontrato o mês de referência ZEUS.', 16, 1)   
		            END CATCH 
              if (@Mes is null) or (@Ano is null)
              begin
                 RAISERROR('Não possível executar o procedimento solicitado! Não foi encontrato o mês, ou ano de referência ZEUS, ', 16, 1)   
                 set @RC = -1
              end   
	            if (@RC != -1)
		            select 
		            'ORC_' col01,  --cod_sistema	
		            '5' col02,     --tipo_movimento	
		            @Ano col03,    --ano_orcamento	
		            @Mes col04,    --mes_orcamento	
		            cr.cod_empresa col05, --cod_empresa	
		            cr.COD_CENTRO_RESP col06, --cod_centro_resp	
		            ti.CodigoReservado col07, --cod_proces_proj	
		            opc.CONTA_COD col08, --cod_grupo_conta	
		            '0' col09, --qtd_real_mes	
                    case when @tipo = 'T' then coa.ValorTransposto 
                         when @tipo = 'S' then coa.ValorSuplemento 
                         when @tipo = 'D' then coa.ValorProposto 
                    end col10, --val_real_mes	
    						    '0' col11, --status_registro 
                    '{4}' col12, --nome_txt	
                    '' col13, --ano_plano	
                    '' col14, --cod_conta_contabil	
                    '' col15, --cod_entrada	
                    GETDATE() col16, --dat_atualizacao	
                    @Ano col17, --cod_pln_uni	
                    @Ano col18, --cod_pln_pro	   
                    @Ano col19  --cod_pln_cta		              
                      from {0}.{1}.CronogramaOrcamentarioAcao coa inner join
                           {0}.{1}.orc_planoContas opc on opc.SeqPlanoContas = coa.SeqPlanoContas inner join
                           {0}.{1}.tai02_AcoesIniciativa ti on ti.CodigoAcao = coa.CodigoAcao and
                                               ti.CodigoProjeto = coa.CodigoProjeto inner join
                           {0}.{1}.Projeto p on p.CodigoProjeto = coa.CodigoProjeto inner join
                           {0}.{1}.UnidadeNegocio un on un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio left join 
                           {0}.{1}.orc_cr cr on cr.cod_proc_proj = ti.CodigoReservado and cr.ano_orcamento = opc.Ano
            end     
            ", bancodb, Ownerdb, codigoEntidade, tipo, arquivo);
        return getDataSet(comandoSQL);
    }


    #endregion

    #region PREFEITURA DE BELO HORIZONTE-MG - PBH

    #region PBH - Planilha Custos

    public DataSet getPlanilhaCustosProjeto(int codigoProjeto, int linhaBase, string where)
    {
        string comandoSQL = string.Format(@"            
               SELECT * 
                 FROM {0}.{1}.f_pbh_GetItensOrcamentoProjeto({2}, {3})
                WHERE 1 = 1
                  {4}
            ", bancodb, Ownerdb, codigoProjeto, linhaBase, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getLinhasBasesPlanilhaCustosProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"            
                SELECT lop.CodigoLinhaBase, 'Versão ' + CONVERT(VarChar, lop.VersaoLinhaBase) + ' - ' + IdentificacaoLinhaBase AS VersaoLinhaBase
                      ,StatusAprovacao, DataStatusAprovacao
                  FROM {0}.{1}.pbh_LinhaBaseOrcamentoProjeto lop
                 WHERE lop.CodigoProjeto = {2}
                  {3}
                 ORDER BY lop.VersaoLinhaBase DESC
            ", bancodb, Ownerdb, codigoProjeto, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getGruposRecursosPlanilhaCustosProjeto(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT item.CodigoGrupoRecurso AS CodigoItem, item.DescricaoGrupo AS NomeItem, grupo.DescricaoGrupo AS NomeGrupo, item.DetalheGrupo
                      FROM GrupoRecurso item INNER JOIN 
			               GrupoRecurso grupo ON grupo.CodigoGrupoRecurso = item.GrupoRecursoSuperior
                     WHERE item.NivelGrupo = 3
                       AND item.CodigoEntidade = {2}
                       {3}
                    ORDER BY grupo.DescricaoGrupo, item.DescricaoGrupo
                    ", bancodb, Ownerdb, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getAnosPlanilhaCustosProjeto(string where)
    {
        comandoSQL = string.Format(@"
                    SELECT Ano 
                      FROM {0}.{1}.pbh_PlanoInvestimento 
                     WHERE Ano >= YEAR(GetDate())
                       {2}
                     ORDER BY Ano DESC
                    ", bancodb, Ownerdb, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public int getAnoCorrentePlanilhaCustosProjeto(int codigoProjeto, int linhaBase)
    {
        int anoCorrente = -1;

        string comandoSQL = string.Format(@"            
               SELECT ISNULL({0}.{1}.f_pbh_GetAnoCorrenteOrcamentoProjeto({2}, {3}), -1) AS AnoCorrente
            ", bancodb, Ownerdb, codigoProjeto, linhaBase);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            anoCorrente = int.Parse(ds.Tables[0].Rows[0]["AnoCorrente"].ToString());

        return anoCorrente;
    }

    public string getCustoUnitarioGrupoRecurso(int codigoGrupo)
    {
        string valorUnitario = "";

        string comandoSQL = string.Format(@"            
               SELECT {0}.{1}.f_pbh_GetCustoUnitarioGrupo({2}) AS CustoUnitario
            ", bancodb, Ownerdb, codigoGrupo);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            valorUnitario = ds.Tables[0].Rows[0]["CustoUnitario"].ToString();

        return valorUnitario;
    }

    public bool incluiItemPlanilhaCustosProjeto(int codigoProjeto, int codigoItem, string quantidade, string valorUnitario
        , string valorTotal, string codigoFonteRecursos, string dotacaoOrcamentaria, string indicaContratarItem, int codigoUsuarioLogado
        , string valorRequeridoAnoCorrente, string valorRequeridoAnoSeguinte, int anoCorrente, string valorRequeridoAnoSeguinte2, string comentario)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @SequenciaRegistro int
                               ,@CodigoItemOrcamento int

                        SELECT @SequenciaRegistro = ISNULL(MAX(SequenciaRegistro), 1) 
                          FROM {0}.{1}.pbh_ItemOrcamentoProjeto
                         WHERE CodigoProjeto = {2}

                        INSERT INTO {0}.{1}.[pbh_ItemOrcamentoProjeto]
                                   ([CodigoProjeto]
                                   ,[SequenciaRegistro]
                                   ,[CodigoGrupoRecurso]
                                   ,[QuantidadeOrcada]
                                   ,[ValorUnitario]
                                   ,[ValorTotal]
                                   ,[CodigoFonteRecursosFinanceiros]
                                   ,[DotacaoOrcamentaria]
                                   ,[IndicaContratarItem]
                                   ,[DataInclusao]
                                   ,[CodigoUsuarioInclusao]
                                   ,[DescricaoItemOrcamentoProjeto])
                             VALUES
                                   ({2}
                                   ,@SequenciaRegistro
                                   ,{3}
                                   ,{4}
                                   ,{5}
                                   ,{6}
                                   ,{7}
                                   ,{8}
                                   ,'{9}'
                                   ,GetDate()
                                   ,{10}
                                   ,'{17}')

                        SET @CodigoItemOrcamento = SCOPE_IDENTITY();

                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{13}
                                   ,{11})

                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{14}
                                   ,{12})

                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{16}
                                   ,{15})

                    END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoItem, quantidade, valorUnitario, valorTotal, codigoFonteRecursos, dotacaoOrcamentaria, indicaContratarItem, codigoUsuarioLogado
                         , valorRequeridoAnoCorrente, valorRequeridoAnoSeguinte, anoCorrente, (anoCorrente + 1), valorRequeridoAnoSeguinte2, (anoCorrente + 2), comentario.Replace("'", "''"));

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaItemPlanilhaCustosProjeto(int codigoItemOrcamento, int codigoItem, string quantidade, string valorUnitario
        , string valorTotal, string codigoFonteRecursos, string dotacaoOrcamentaria, string indicaContratarItem, int codigoUsuarioLogado
        , string valorRequeridoAnoCorrente, string valorRequeridoAnoSeguinte, int anoCorrente, string valorRequeridoAnoSeguinte2, string comentario)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @CodigoItemOrcamento int

                         SET @CodigoItemOrcamento = {2};

                        UPDATE {0}.{1}.[pbh_ItemOrcamentoProjeto]
                           SET [CodigoGrupoRecurso] = {3}
                              ,[QuantidadeOrcada] = {4}
                              ,[ValorUnitario] = {5}
                              ,[ValorTotal] = {6}
                              ,[CodigoFonteRecursosFinanceiros] = {7}
                              ,[DotacaoOrcamentaria] = {8}
                              ,[IndicaContratarItem] = '{9}'
                              ,[DataAlteracao] = GetDate()
                              ,[CodigoUsuarioAlteracao] = {10}
                              ,[DescricaoItemOrcamentoProjeto] = '{17}'
                         WHERE CodigoItemOrcamento = @CodigoItemOrcamento
                    
                    IF EXISTS(SELECT 1 FROM {0}.{1}.[pbh_itemOrcamentoAno] WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento AND [Ano] = {13}) 
                      BEGIN
                        UPDATE {0}.{1}.[pbh_itemOrcamentoAno] SET [ValorRequeridoAno] = {11}
                         WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento
                           AND [Ano] = {13}
                      END
                     ELSE
                     BEGIN
                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{13}
                                   ,{11})
                     END

                     IF EXISTS(SELECT 1 FROM {0}.{1}.[pbh_itemOrcamentoAno] WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento AND [Ano] = {14}) 
                      BEGIN
                        UPDATE {0}.{1}.[pbh_itemOrcamentoAno] SET [ValorRequeridoAno] = {12}
                         WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento
                           AND [Ano] = {14}
                      END
                     ELSE
                     BEGIN
                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{14}
                                   ,{12})
                     END

                     IF EXISTS(SELECT 1 FROM {0}.{1}.[pbh_itemOrcamentoAno] WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento AND [Ano] = {16}) 
                      BEGIN
                        UPDATE {0}.{1}.[pbh_itemOrcamentoAno] SET [ValorRequeridoAno] = {15}
                         WHERE [CodigoItemOrcamento] = @CodigoItemOrcamento
                           AND [Ano] = {16}
                      END
                     ELSE
                     BEGIN
                        INSERT INTO {0}.{1}.[pbh_itemOrcamentoAno]
                                   ([CodigoItemOrcamento]
                                   ,[Ano]
                                   ,[ValorRequeridoAno])
                             VALUES
                                   (@CodigoItemOrcamento
                                   ,{16}
                                   ,{15})
                     END
                    END
                        ", bancodb, Ownerdb
                         , codigoItemOrcamento, codigoItem, quantidade, valorUnitario, valorTotal, codigoFonteRecursos, dotacaoOrcamentaria, indicaContratarItem, codigoUsuarioLogado
                         , valorRequeridoAnoCorrente, valorRequeridoAnoSeguinte, anoCorrente, (anoCorrente + 1), valorRequeridoAnoSeguinte2, (anoCorrente + 2), comentario.Replace("'", "''"));

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiItemPlanilhaCustosProjeto(int codigoItemOrcamento, int codigoUsuarioLogado)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @CodigoItemOrcamento int

                         SET @CodigoItemOrcamento = {2};

                        UPDATE {0}.{1}.[pbh_ItemOrcamentoProjeto]
                           SET [DataExclusao] = GetDate()
                              ,[CodigoUsuarioExclusao] = {3}
                         WHERE CodigoItemOrcamento = @CodigoItemOrcamento

                    END
                        ", bancodb, Ownerdb
                         , codigoItemOrcamento, codigoUsuarioLogado);

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region PBH - Administração de Plano de Investimento
    public DataSet getPlanoInvestimento(string where)
    {

        comandoSQL = string.Format(@"
            SELECT pi.CodigoPlanoInvestimento
                  ,pi.DescricaoPlanoInvestimento
                  ,pi.Ano
                  ,pi.DataInicioInclusaoProjeto
                  ,pi.DataFinalInclusaoProjetos
                  ,pi.CodigoStatusPlanoInvestimento
                  ,spi.DescricaoStatusPlanoInvestimento
              FROM {0}.{1}.pbh_PlanoInvestimento pi
        INNER JOIN {0}.{1}.pbh_StatusPlanoInvestimento spi on spi.CodigoStatusPlanoInvestimento = pi.CodigoStatusPlanoInvestimento
 {2}", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiPlanoInvestimento(string descricaoPlanoInvestimento, int ano, string dataInicioInclusaoProjeto, string dataFinalInclusaoProjetos, int codigoStatusPlanoInvestimento, ref int novoCodPlanoInvestimento, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @DescricaoPlanoInvestimento as varchar(100)
                            declare @ano as int
                            DECLARE @DataInicioInclusaoProjeto as datetime
                            DECLARE @DataFinalInclusaoProjetos as datetime
                            DECLARE @CodigoStatusPlanoInvestimento as int
                            DECLARE @codigoPlanoInvestimento as int
                            
                            
                            SET @DescricaoPlanoInvestimento = '{2}'
                            SET @ano = {3}
                            SET @DataInicioInclusaoProjeto = convert(datetime,'{4}', 103)
                            SET @DataFinalInclusaoProjetos = convert(datetime,'{5}', 103)
                            SET @CodigoStatusPlanoInvestimento = {6}

                            INSERT INTO {0}.{1}.pbh_PlanoInvestimento
                                        (DescricaoPlanoInvestimento ,Ano
                                         ,DataInicioInclusaoProjeto
                                         ,DataFinalInclusaoProjetos,CodigoStatusPlanoInvestimento)
                               VALUES(@DescricaoPlanoInvestimento,@ano
                                      ,@DataInicioInclusaoProjeto
                                      ,@DataFinalInclusaoProjetos,@CodigoStatusPlanoInvestimento)

                            SELECT @codigoPlanoInvestimento = SCOPE_IDENTITY()

                            SELECT @codigoPlanoInvestimento AS NovoCodigoPlanoInvestimento

                          END
                        ", bancodb, Ownerdb,
                          descricaoPlanoInvestimento, ano, dataInicioInclusaoProjeto, dataFinalInclusaoProjetos, codigoStatusPlanoInvestimento);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodPlanoInvestimento = int.Parse(ds.Tables[0].Rows[0]["NovoCodigoPlanoInvestimento"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaPlanoInvestimento(int codigoPlanoInvestimento, string descricaoPlanoInvestimento, int ano, string dataInicioInclusaoProjeto, string dataFinalInclusaoProjetos, int codigoStatusPlanoInvestimento, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @DescricaoPlanoInvestimento as varchar(100)
                            declare @ano as int
                            DECLARE @DataInicioInclusaoProjeto as datetime
                            DECLARE @DataFinalInclusaoProjetos as datetime
                            DECLARE @CodigoStatusPlanoInvestimento as int
                            DECLARE @codigoPlanoInvestimento as int
                            
                            SET @codigoPlanoInvestimento = {2}
                            SET @DescricaoPlanoInvestimento = '{3}'
                            SET @ano = {4}
                            SET @DataInicioInclusaoProjeto = convert(datetime,'{5}', 103)
                            SET @DataFinalInclusaoProjetos = convert(datetime,'{6}', 103)
                            SET @CodigoStatusPlanoInvestimento = {7}
                            
                            
                            UPDATE {0}.{1}.pbh_PlanoInvestimento
                            SET DescricaoPlanoInvestimento = @DescricaoPlanoInvestimento
                               ,Ano = @ano
                               ,DataInicioInclusaoProjeto = @DataInicioInclusaoProjeto
                               ,DataFinalInclusaoProjetos = @DataFinalInclusaoProjetos
                               ,CodigoStatusPlanoInvestimento = @CodigoStatusPlanoInvestimento
                            WHERE CodigoPlanoInvestimento = @codigoPlanoInvestimento
                        END", bancodb, Ownerdb,
                             codigoPlanoInvestimento, descricaoPlanoInvestimento, ano, dataInicioInclusaoProjeto, dataFinalInclusaoProjetos, codigoStatusPlanoInvestimento);

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiPlanoInvestimento(int codigoPlanoInvestimemnto, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoPlanoInvestimento int
                            SET @CodigoPlanoInvestimento = {2}

                            DELETE FROM {0}.{1}.pbh_PlanoInvestimento
                             WHERE CodigoPlanoInvestimento = @CodigoPlanoInvestimento                           
                        END", bancodb, Ownerdb
                         , codigoPlanoInvestimemnto);

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
            if (regAf == 0)
            {
                mensagemErro = "Nenhum registro foi excluído.";
            }
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }
    #endregion

    #endregion

    #region Relatório Diário de Obra (RDO)

    public DataSet getAgrupamentoItensRDO(string where)
    {
        string comandoSQL = string.Format(@"            
               SELECT * 
                 FROM {0}.{1}.Rdo_AgrupamentoItens
                WHERE 1 = 1
                  {2}
            ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getItensRDO(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"            
                SELECT * 
                  FROM Rdo_Itens
                 WHERE CodigoProjeto = {2}
                   AND DataExclusao IS NULL
                   {3}
                 ORDER BY NumeroOrdem
            ", bancodb, Ownerdb, codigoProjeto, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiItemRDO(int codigoProjeto, int codigoAgrupamento, string descricaoItem, int numeroOrdem, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoProjeto int
                                   ,@CodigoAgrupamento int
                                   ,@CodigoItem int
                                   ,@NumeroOrdem int
                                   ,@DescricaoItem VarChar(132)

                            SET @CodigoProjeto = {2}
                            SET @CodigoAgrupamento = {3}
                            SET @DescricaoItem = '{4}'
                            SET @NumeroOrdem = {5}

                            UPDATE {0}.{1}.Rdo_Itens SET NumeroOrdem = (NumeroOrdem + 1)
                             WHERE NumeroOrdem >= @NumeroOrdem
                               AND CodigoProjeto = @CodigoProjeto
                               AND CodigoAgrupamento = @CodigoAgrupamento
                               AND DataExclusao IS NULL
                            
                            INSERT INTO {0}.{1}.Rdo_Itens
                                       (CodigoProjeto
                                       ,CodigoAgrupamento
                                       ,DescricaoItem
                                       ,NumeroOrdem
                                       ,DataInclusao)
                                 VALUES
                                       (@CodigoProjeto
                                       ,@CodigoAgrupamento
                                       ,@DescricaoItem
                                       ,@NumeroOrdem
                                       ,GetDate())

                            SELECT @CodigoItem = SCOPE_IDENTITY()                            

                            SELECT @CodigoItem AS CodigoItem

                        END
                        ", bancodb, Ownerdb
                         , codigoProjeto, codigoAgrupamento, descricaoItem.Replace("'", "''"), numeroOrdem);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoItem"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaItemRDO(int codigoItem, string descricaoItem, int numeroOrdem, int codigoAgrupamento, int codigoProjeto)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoProjeto int
                                   ,@CodigoAgrupamento int
                                   ,@CodigoItem int
                                   ,@NumeroOrdemAtual int
                                   ,@NumeroOrdem int
                                   ,@DescricaoItem VarChar(132)
                            
                            SET @CodigoItem = {2}
                            SET @DescricaoItem = '{3}'
                            SET @NumeroOrdem = {4}
                            SET @CodigoProjeto = {5}
                            SET @CodigoAgrupamento = {6}

                            SELECT @NumeroOrdemAtual = NumeroOrdem
                              FROM {0}.{1}.Rdo_Itens
                             WHERE CodigoItem = @CodigoItem

                            IF (@NumeroOrdemAtual > @NumeroOrdem)
                                UPDATE {0}.{1}.Rdo_Itens SET NumeroOrdem = (NumeroOrdem + 1)
                                 WHERE NumeroOrdem < @NumeroOrdemAtual
                                   AND NumeroOrdem >= @NumeroOrdem
                                   AND CodigoProjeto = @CodigoProjeto
                                   AND CodigoAgrupamento = @CodigoAgrupamento
                                   AND DataExclusao IS NULL 
                            ELSE IF (@NumeroOrdemAtual < @NumeroOrdem)
                                UPDATE {0}.{1}.Rdo_Itens SET NumeroOrdem = (NumeroOrdem - 1)
                                 WHERE NumeroOrdem > @NumeroOrdemAtual
                                   AND NumeroOrdem <= @NumeroOrdem
                                   AND CodigoProjeto = @CodigoProjeto
                                   AND CodigoAgrupamento = @CodigoAgrupamento
                                   AND DataExclusao IS NULL 

                            UPDATE {0}.{1}.Rdo_Itens 
                               SET NumeroOrdem = @NumeroOrdem
                                  ,DescricaoItem = @DescricaoItem
                            WHERE CodigoItem = @CodigoItem

                        END
                        ", bancodb, Ownerdb
                         , codigoItem
                         , descricaoItem.Replace("'", "''")
                         , numeroOrdem
                         , codigoProjeto
                         , codigoAgrupamento);

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiItemRDO(int codigoItem, int codigoAgrupamento, int codigoProjeto)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoProjeto int
                                   ,@CodigoAgrupamento int
                                   ,@CodigoItem int
                                   ,@NumeroOrdemAtual int

                            SET @CodigoItem = {2}
                            SET @CodigoProjeto = {3}
                            SET @CodigoAgrupamento = {4}

                            SELECT @NumeroOrdemAtual = NumeroOrdem
                              FROM {0}.{1}.Rdo_Itens
                             WHERE CodigoItem = @CodigoItem

                            UPDATE {0}.{1}.Rdo_Itens SET NumeroOrdem = (NumeroOrdem - 1)
                             WHERE NumeroOrdem >= @NumeroOrdemAtual
                               AND CodigoProjeto = @CodigoProjeto
                               AND CodigoAgrupamento = @CodigoAgrupamento
                               AND DataExclusao IS NULL 

                            UPDATE {0}.{1}.Rdo_Itens 
                               SET DataExclusao = GetDate()
                            WHERE CodigoItem = @CodigoItem

                        END
                        ", bancodb, Ownerdb
                         , codigoItem
                         , codigoProjeto
                         , codigoAgrupamento);

            int regAf = 0;

            execSQL(comandoSQL, ref regAf);


            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region Imagens

    public byte[] GetImageThumbnail(object image, int width, int height)
    {
        string utilizaThumbnail = "N";

        DataSet ds = getParametrosSistema("UtilizaThumbnail");

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            utilizaThumbnail = ds.Tables[0].Rows[0]["UtilizaThumbnail"].ToString();
        }

        if (utilizaThumbnail == "S")
        {
            //create image from array of bytes----------------

            byte[] array = image as byte[];
            MemoryStream imageStream = new MemoryStream(array);
            System.Drawing.Image img = System.Drawing.Image.FromStream(imageStream);

            //------------------------------------------------

            //create thumbnail--------------------------------

            img = img.GetThumbnailImage(width, height, new System.Drawing.Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero);

            //--------------------------------------------------

            //save thumnail to new stream---------------------
            MemoryStream thumbnailStream = new MemoryStream(array);
            img.Save(thumbnailStream, System.Drawing.Imaging.ImageFormat.Bmp);

            //------------------------------------------------
            return thumbnailStream.ToArray();
        }
        else
        {
            MemoryStream ms = new MemoryStream((byte[])image);

            Size sz = new Size(width, height);

            System.Drawing.Image imageRedimencionada = resizeImage(System.Drawing.Image.FromStream(ms), sz);

            MemoryStream ms2 = new MemoryStream();

            imageRedimencionada.Save(ms2, ImageFormat.Bmp);

            return ms2.ToArray();
        }

    }

    public bool ThumbnailCallback()
    {
        return true;
    }

    private static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
    {
        Bitmap b = new Bitmap(size.Width, size.Height);
        Graphics g = Graphics.FromImage((System.Drawing.Image)b);
        //g.InterpolationMode = InterpolationMode.High;
        g.CompositingQuality = CompositingQuality.HighSpeed;
        g.SmoothingMode = SmoothingMode.HighSpeed;
        g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
        g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
        g.Dispose();

        return (System.Drawing.Image)b;
    }


    #endregion


    #region Relatório de Desempenho de Carteiras, Programas e Projetos

    public string getGraficoCurvaSRelatorio(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        string indicaMostraLinhaLB1 = "", indicaMostraLinhaTendencia = "", termoTendencia = "";
        DataSet ds = getParametrosSistema("mostraLB1CurvaSFisica", "mostraTendenciaCurvaSFisica", "labelProgramacaoCurvaSFisica");

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            indicaMostraLinhaLB1 = ds.Tables[0].Rows[0]["mostraLB1CurvaSFisica"].ToString();
            indicaMostraLinhaTendencia = ds.Tables[0].Rows[0]["mostraTendenciaCurvaSFisica"].ToString();
            termoTendencia = ds.Tables[0].Rows[0]["labelProgramacaoCurvaSFisica"].ToString();
        }

        if (string.IsNullOrEmpty(indicaMostraLinhaLB1))
            indicaMostraLinhaLB1 = "S";

        if (string.IsNullOrEmpty(indicaMostraLinhaTendencia))
            indicaMostraLinhaTendencia = "S";

        if (string.IsNullOrEmpty(termoTendencia))
            termoTendencia = "Programado";

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""0"" numberSuffix=""%""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1"" yAxisMaxValue=""100"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" slantLabels=""1"" labelDisplay=""ROTATE"" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""{1}"" color=""{0}"" showValues=""0"">", corPrevisto, Resources.traducao.previsto));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""{2}: {1}%"" /> ", valor == "" ? "" : valor, displayValue, Resources.traducao.previsto));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));


        xml.Append(string.Format(@"<dataset seriesName=""{1}"" color=""{0}"" showValues=""0"">", corReal, Resources.traducao.realizado));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorReal"].ToString();

                string dashed = dt.Rows[i]["IndicaValorRealProjetado"].ToString();
                dashed = dashed == "S" ? "1" : "0";
                string projetado = dashed == "1" ? " (projetado)" : "";
                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""{4}: {1}%{3}""  dashed=""{2}""/> ", valor == "" ? "" : valor, displayValue, dashed, projetado, Resources.traducao.realizado));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        if (indicaMostraLinhaLB1 == "S")
        {

            xml.Append(string.Format(@"<dataset seriesName=""{1}"" color=""{0}"" showValues=""0"">", corPrevistoLB1, Resources.traducao.previsto_1__lb));

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    string valor = dt.Rows[i]["ValorPrevistoLB1"].ToString();

                    string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                    xml.Append(string.Format(@"<set value=""{0}"" toolText=""{2}: {1}%"" /> ", valor == "" ? "" : valor, displayValue, Resources.traducao.previsto_1__lb));
                }
            }
            catch
            {
                return "";
            }

            xml.Append(string.Format(@"</dataset>"));
        }

        if (indicaMostraLinhaTendencia == "S")
        {

            xml.Append(string.Format(@"<dataset seriesName=""{1}"" color=""{0}"" showValues=""0"">", corTendencia, termoTendencia));

            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    string valor = dt.Rows[i]["ValorPrevistoTendencia"].ToString();

                    string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));

                    xml.Append(string.Format(@"<set value=""{0}"" toolText=""{2}: {1}%"" /> ", valor == "" ? "" : valor, displayValue, termoTendencia));
                }
            }
            catch
            {
                return "";
            }

            xml.Append(string.Format(@"</dataset>"));
        }

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public string getGraficoCurvaSFinanceiraRelatorio(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F7F7F7"" labelPadding=""0"" numberSuffix=""%""
                                    inDecimalSeparator="","" scrollHeight=""{3}"" showLegend=""1"" yAxisMaxValue=""100"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" slantLabels=""1"" labelDisplay=""ROTATE"" >", usarGradiente + usarBordasArredondadas + exportar
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));
        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" color=""{0}"" showValues=""0"">", corPrevisto));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorPrevisto"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorPrevistoPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["ValorPrevistoAcumulado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayPeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n2}", double.Parse(valorPeriodo));
                string displayAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n2}", double.Parse(valorAcumulado));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto: {1}%{2}{3}"" /> ", valor == "" ? "" : valor, displayValue, "{br}Previsto no Período: " + displayPeriodo, "{br}Previsto Acumulado: " + displayAcumulado));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));


        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" color=""{0}"" showValues=""0"">", corReal));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["ValorReal"].ToString();
                string valorPeriodo = dt.Rows[i]["ValorRealPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["ValorRealAcumulado"].ToString();

                if (valor != "")
                {
                    string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                    string displayPeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n2}", double.Parse(valorPeriodo));
                    string displayAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n2}", double.Parse(valorAcumulado));

                    xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado: {1}%{2}{3}"" /> ", valor == "" ? "" : valor, displayValue, "{br}Realizado no Período: " + displayPeriodo, "{br}Realizado Acumulado: " + displayAcumulado));
                }
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public DataSet getCurvaSRelatorio(int codigoEntidade, int codigoUsuario, int codigoGrupo, string tipoGrupo, int codigoUnidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT Periodo, ValorPrevisto * 100 AS ValorPrevisto, ValorReal * 100 AS ValorReal
                    , ValorPrevistoLB1 * 100 AS ValorPrevistoLB1, ValorPrevistoTendencia * 100 AS ValorPrevistoTendencia, IndicaValorRealProjetado 
                FROM {0}.{1}.f_getCurvaSFisicaGrupoProjeto({2}, {3}, {4}, '{5}', {6})
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoGrupo, tipoGrupo, codigoUnidade, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Gestão de Painéis

    public DataSet getGraficosDisponiveisPainel(int codigoEntidade, string iniciaisPainel)
    {
        string comandoSQL = string.Format(
            @"SELECT [CodigoGrafico]
                    ,[UrlGrafico]
                    ,[DescricaoGrafico]
                    ,[IniciaisPainel]
                    ,[TituloGrafico]
                    ,[PosicaoDefault] AS Posicao
                    ,[ColSpan]
                    ,[RowSpan]
                FROM {0}.{1}.[Graficos]
                WHERE [IniciaisPainel] = '{3}'
                AND [CodigoEntidade] = {2}
                ORDER BY TituloGrafico
               ", bancodb, Ownerdb, codigoEntidade, iniciaisPainel);

        return getDataSet(comandoSQL);
    }

    public DataSet getPainelUsuario(int codigoEntidade, int codigoUsuario, string iniciaisPainel)
    {
        string comandoSQL = string.Format(
            @"SELECT gu.CodigoGrafico
			        ,g.UrlGrafico
			        ,g.IniciaisPainel
			        ,g.TituloGrafico
			        ,gu.Posicao
			        ,gu.ColSpan
			        ,gu.RowSpan 
	           FROM {0}.{1}.GraficosUsuario gu INNER JOIN
			        Graficos g ON g.CodigoGrafico = gu.CodigoGrafico
              WHERE gu.Posicao IS NOT NULL
                AND g.IniciaisPainel = '{4}'
                AND g.CodigoEntidade = {2}
                AND gu.CodigoUsuario = {3}
              ORDER BY gu.Posicao
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, iniciaisPainel);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            return ds;
        }

        comandoSQL = string.Format(
            @"SELECT [CodigoGrafico]
                    ,[UrlGrafico]
                    ,[DescricaoGrafico]
                    ,[IniciaisPainel]
                    ,[TituloGrafico]
                    ,[PosicaoDefault] AS Posicao
                    ,[ColSpan]
                    ,[RowSpan]
                FROM {0}.{1}.[Graficos]
                WHERE [PosicaoDefault] IS NOT NULL
                AND [IniciaisPainel] = '{4}'
                AND [CodigoEntidade] = {2}
                ORDER BY PosicaoDefault
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, iniciaisPainel);

        return getDataSet(comandoSQL);
    }

    public DataSet getConfiguracoesPainelUsuario(int codigoEntidade, int codigoUsuario, string iniciaisPainel)
    {
        string comandoSQL = string.Format(
            @"SELECT SUM(gu.ColSpan) AS NumeroColunas,
                     SUM(gu.RowSpan) AS NumeroLinhas
	           FROM {0}.{1}.GraficosUsuario gu INNER JOIN
			        Graficos g ON g.CodigoGrafico = gu.CodigoGrafico
              WHERE gu.Posicao IS NOT NULL
                AND g.IniciaisPainel = '{4}'
                AND g.CodigoEntidade = {2}
                AND gu.CodigoUsuario = {3}
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, iniciaisPainel);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            return ds;
        }

        comandoSQL = string.Format(
            @"SELECT SUM(ColSpan) AS NumeroColunas,
                     SUM(RowSpan) AS NumeroLinhas
                FROM {0}.{1}.[Graficos]
                WHERE [PosicaoDefault] IS NOT NULL
                AND [IniciaisPainel] = '{4}'
                AND [CodigoEntidade] = {2}
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, iniciaisPainel);

        return getDataSet(comandoSQL);
    }

    public void geraGraficosPainel(int codigoEntidade, int codigoUsuario, string iniciaisPainel, int numeroColunas, int numeroLinhas, int alturaTela, HtmlGenericControl DivGrafico)
    {
        string innerTable = "";
        string linha1 = "";
        string linha2 = "";

        int numeroColunasLinha1 = numeroColunas;
        int numeroColunasLinha2 = numeroColunas;

        DataSet ds = getPainelUsuario(codigoEntidade, codigoUsuario, iniciaisPainel);


        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            int indexAtual = 0;

            bool possuiLinha2 = false;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int posicaoAtual = int.Parse(dr["Posicao"].ToString());

                if (numeroColunasLinha1 > 0)
                {
                    int colSpan = int.Parse(dr["ColSpan"].ToString());
                    int rowSpan = int.Parse(dr["RowSpan"].ToString());

                    numeroColunasLinha1 = numeroColunasLinha1 - colSpan;

                    if (rowSpan > 1)
                    {
                        numeroColunasLinha2 = numeroColunasLinha2 - colSpan;
                    }
                }
                else
                {
                    if (numeroColunasLinha2 > 0)
                    {
                        possuiLinha2 = true;
                    }
                }

                indexAtual++;

                if (indexAtual >= (numeroColunas * numeroLinhas))
                    break;
            }

            numeroColunasLinha1 = numeroColunas;
            numeroColunasLinha2 = numeroColunas;
            indexAtual = 0;

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int posicaoAtual = int.Parse(dr["Posicao"].ToString());

                int colSpan = int.Parse(dr["ColSpan"].ToString());
                int rowSpan = int.Parse(dr["RowSpan"].ToString());
                string url = getPathSistema() + dr["UrlGrafico"] + "?Altura=" + (!possuiLinha2 || rowSpan == 2 ? (alturaTela + 10) : alturaTela / 2) + "&Titulo=" + Server.UrlEncode(dr["TituloGrafico"].ToString());

                if (numeroColunasLinha1 > 0)
                {
                    numeroColunasLinha1 = numeroColunasLinha1 - colSpan;

                    if (rowSpan > 1)
                    {
                        numeroColunasLinha2 = numeroColunasLinha2 - colSpan;
                    }

                    linha1 += string.Format(@"<td style=""padding:5px"" colSpan=""{1}"" rowSpan=""{2}"">
                                                <iframe frameborder=""0"" height=""{3}"" scrolling=""no"" src=""{0}""
                                                    width=""100%""></iframe>
                                            </td>", url
                                                  , colSpan
                                                  , rowSpan
                                                 , !possuiLinha2 || rowSpan == 2 ? (alturaTela + 10) : (alturaTela / 2));
                }
                else
                {
                    if (numeroColunasLinha2 > 0)
                    {
                        numeroColunasLinha2 = numeroColunasLinha2 - colSpan;

                        linha2 += string.Format(@"<td style=""padding:5px"" colSpan=""{1}"" rowSpan=""{2}"">
                                                <iframe frameborder=""0"" height=""{3}"" scrolling=""no"" src=""{0}""
                                                    width=""100%""></iframe>
                                            </td>", url
                                                  , colSpan
                                                  , rowSpan
                                                 , !possuiLinha2 ? 0 : (alturaTela / 2));
                    }
                }

                indexAtual++;

                if (indexAtual >= (numeroColunas * numeroLinhas))
                    break;
            }

            innerTable = string.Format(@"<TABLE style=""WIDTH: 100%; height:{2}px"" cellSpacing=0 cellPadding=0>
                                          <tr style=""WIDTH: 100%; height:{3}px"">{0}</tr>
                                          <tr style=""WIDTH: 100%; height:{4}px"">{1}</tr>
                                        </TABLE>", linha1
                                                 , linha2
                                                 , alturaTela
                                                 , !possuiLinha2 ? alturaTela : (alturaTela / 2)
                                                 , !possuiLinha2 ? 0 : (alturaTela / 2));

            DivGrafico.InnerHtml = innerTable;
        }
    }

    #endregion

    #region Medição

    public DataSet getListaItensMedicao(int codigoMedicao, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT * 
                  FROM {0}.{1}.f_GetItensMedicao({2})  
                 WHERE 1 = 1
                   {3}   
                 ORDER BY StringOrdenacao         
               ", bancodb, Ownerdb, codigoMedicao, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getValoresAcrescimosRetencoesContrato(int codigoMedicao, string codigoContrato)
    {
        string comandoSQL = string.Format(
              @"SELECT * 
                  FROM {0}.{1}.f_GetValoresAcrescimosRetencoesContrato2({2}, {3})  
                 WHERE 1 = 1
                 ORDER BY OrdemItem         
               ", bancodb, Ownerdb, codigoContrato, codigoMedicao);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Fator Chave

    public DataSet getProjetosFatorChave(int codigoFatorChave, string iniciaisObjeto, int codigoUsuario, int ano)
    {
        string comandoSQL = string.Format(@"
                SELECT * FROM {0}.{1}.f_cni_getProjetosObjetoPorAno({2}, '{3}', {4}, {5})
                    ORDER BY IndicaProjetoCarteiraPrioritaria DESC", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto, codigoUsuario, ano);
        return getDataSet(comandoSQL);
    }

    public DataSet getProjetosFatorChave2(int codigoFatorChave, string iniciaisObjeto, int codigoUsuario, int ano)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT CodigoProjeto, NomeProjeto, CorDesempenho, PercentualConcluido, PodeAcessarProjeto, IndicaProjetoCarteiraPrioritaria FROM {0}.{1}.f_cni_getProjetosObjetoPorAno({2}, '{3}', {4}, {5})
                    ORDER BY IndicaProjetoCarteiraPrioritaria DESC", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto, codigoUsuario, ano);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosMetaFatorChave(int codigoFatorChave, string iniciaisObjeto)
    {
        string comandoSQL = string.Format(@"
                EXEC {0}.{1}.p_cni_getDadosMeta {2}, '{3}'", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getQuadroSinteseFatorChave(int codigoFator, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT * FROM {0}.{1}.f_cni_getQuadroSintese({2}, {3})
        ", bancodb, Ownerdb, codigoFator, codigoUsuario, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getPeriodicidadeIndicadorFatorChave(int codigoUnidade, int codigoIndicador, string where)
    {
        string comandoSQL = string.Format(@"
            EXEC {0}.{1}.p_cni_getIndicadorPeriodicidade {2}, {3}, 'A'
        ", bancodb, Ownerdb, codigoUnidade, codigoIndicador, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet getAnosAnalisesFatorChave(int codigoFatorChave, string iniciaisObjeto)
    {
        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_cni_getAnosAnaliseObjeto({2}, '{3}') ORDER BY Ano DESC", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosMacrometaAno(int codigoFatorChave, string iniciaisObjeto, int ano)
    {
        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_cni_getAnalisesObjetoPorAno({2}, '{3}', {4}) ORDER BY DataAnalise DESC", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto, ano);
        return getDataSet(comandoSQL);
    }

    public string getGraficoMacrometa(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot, string codigoMeta, string corFatorChave, string unidadeMedida)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        int i = 0;

        string formaNumero = "";

        if (unidadeMedida == "%")
            formaNumero = @" numberSuffix=""%"" ";
        else
        {
            if (unidadeMedida.Contains("$"))
                formaNumero = string.Format(@" numberPrefix=""{0} "" ", unidadeMedida);
            else
                formaNumero = "";
        }

        xml.Append(string.Format(@"<chart  valuePadding=""9""  {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""3"" chartLeftMargin=""1"" chartRightMargin=""1"" chartTopMargin=""5"" canvasBgColor=""F2F2F2"" canvasLeftMargin=""0"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""2"" showvalues=""0"" numVisiblePlot=""{5}"" scrollToEnd=""1"" showBorder=""0"" BgColor=""F2F2F2"" labelDisplay=""NONE"" divLineColor=""FFFFFF""
                                    inDecimalSeparator="","" connectNullData=""0"" scrollHeight=""{3}"" showLegend=""0"" inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" {6}  plotGradientColor=""""
                                    showAlternateHGridColor=""0"">", ""
                                                                                                                                                          , titulo
                                                                                                                                                          , fonte
                                                                                                                                                          , scrollHeight
                                                                                                                                                          , numDivLines
                                                                                                                                                          , numVisiblePlot
                                                                                                                                                          , formaNumero));
        xml.Append(string.Format(@"<categories>"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0}"" />", dt.Rows[i]["Periodo"].ToString()));
            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</categories>"));

        xml.Append(string.Format(@"<dataset lineThickness=""1"" color=""F2F2F2"" anchorSides=""5""  seriesName=""Meta"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorMeta = dt.Rows[i]["ValorMeta"].ToString();
                string ano = dt.Rows[i]["Ano"].ToString();
                string mes = dt.Rows[i]["Mes"].ToString();

                string cor = "FF6666";

                string link = "";

                string displayMeta = "";

                if (unidadeMedida == "%")
                    displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}%", double.Parse(valorMeta));
                else
                {
                    if (unidadeMedida.Contains("$"))
                        displayMeta = valorMeta == "" ? "-" : string.Format(unidadeMedida + "{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                    else
                        displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                }

                xml.Append(string.Format(@"<set alpha=""1"" anchorRadius='9' showValue=""1"" anchorBorderThickness='1' anchorSides='20' anchorBorderColor='000000' displayValue=""{4}"" anchorBgColor='F2F2F2' value=""{0}"" {2} toolText=""Meta: {1}""/> ", valorMeta, displayMeta, link, cor, "Meta{BR}" + dt.Rows[i]["Periodo"].ToString() + "{BR}" + displayMeta));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset lineThickness=""1"" color=""F2F2F2"" anchorSides=""5"" seriesName=""Meta"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valorMeta = dt.Rows[i]["ValorMeta"].ToString();
                string ano = dt.Rows[i]["Ano"].ToString();
                string mes = dt.Rows[i]["Mes"].ToString();

                string cor = "FF6666";

                string link = "";

                string displayMeta = "";

                if (unidadeMedida == "%")
                    displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}%", double.Parse(valorMeta));
                else
                {
                    if (unidadeMedida.Contains("$"))
                        displayMeta = valorMeta == "" ? "-" : string.Format(unidadeMedida + "{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                    else
                        displayMeta = valorMeta == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorMeta));
                }

                xml.Append(string.Format(@"<set alpha=""1"" anchorRadius='5' showValue=""0"" anchorBorderThickness='1' anchorSides='20' anchorBorderColor='000000' anchorBgColor='{3}' value=""{0}"" {2} toolText=""Meta: {1}""/> ", valorMeta, displayMeta, link, cor));

            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Resultado"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {

                string valorResultado = dt.Rows[i]["ValorRealizado"].ToString();
                int ano = int.Parse(dt.Rows[i]["Ano"].ToString());
                int mes = int.Parse(dt.Rows[i]["Mes"].ToString());

                string cor = dt.Rows[i]["CorIndicador"].ToString().ToUpper().Trim();

                switch (cor)
                {
                    case "VERDE":
                        cor = corSatisfatorio;
                        break;
                    case "AMARELO":
                        cor = corAtencao;
                        break;
                    case "VERMELHO":
                        cor = corCritico;
                        break;
                    case "LARANJA":
                        cor = "EC8D00";
                        break;
                    case "AZUL":
                        cor = corExcelente;
                        break;
                    case "BRANCO":
                        cor = "FFFFFF";
                        break;
                }

                string link = "";

                string displayResultado = "";

                if (unidadeMedida == "%")
                    displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}%", double.Parse(valorResultado));
                else
                {
                    if (unidadeMedida.Contains("$"))
                        displayResultado = valorResultado == "" ? "-" : string.Format(unidadeMedida + "{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                    else
                        displayResultado = valorResultado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorResultado));
                }

                string corLinha = @" lineThickness=""1"" color=""F2F2F2"" ";

                if (ano <= 2012)
                {
                    cor = corFatorChave;
                    corLinha = @" lineThickness=""1"" color=""" + corFatorChave + @""" ";
                }

                xml.Append(string.Format(@"<set anchorRadius='5' anchorSides='20' anchorBorderColor='CCCCCC' anchorBgColor='{3}' value=""{0}"" {2} {4} toolText=""Resultado: {1}"" /> ", valorResultado, displayResultado, link, cor, corLinha));


            }

        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontValues\" type=\"font\" face=\"Verdana\" size=\"" + (fonte - 1) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte - 1) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontValues\" />");
        xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public DataSet getTemasFatorChave(int codigoFatorChave, string iniciaisObjeto)
    {
        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_cni_getTemasFatorChave({2})", bancodb, Ownerdb, codigoFatorChave, iniciaisObjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getObjetivosTemaPrioritario(int codigoTema)
    {
        string comandoSQL = string.Format(@"SELECT * FROM {0}.{1}.f_cni_getObjetivosTema({2})", bancodb, Ownerdb, codigoTema);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Layout e Padronização

    #region MEMO

    public void setaTamanhoMaximoMemo(ASPxMemo memo, int tamanhoMaximo, ASPxLabel labelContador, bool calculaAltura = false)
    {
        string traducao_de = Resources.traducao.de;
        string funcaoChange = string.Format(@"{1}.SetText(s.GetInputElement().value.length + ' {3} ' + {0}); ", tamanhoMaximo, labelContador.ClientInstanceName, memo.ClientInstanceName, traducao_de);

        string funcao = "function(){" + funcaoChange + "}";

        string funcaoInit = string.Format(@"{2}.GetInputElement().maxLength = {0}; 
                                            {1}.SetText({2}.GetInputElement().value.length + ' {4} ' + {0}); 
                                            ASPxClientUtils.AttachEventToElement({2}.GetInputElement(), ""input"", {3});
                                            ASPxClientUtils.AttachEventToElement({2}.GetInputElement(), ""change"", {3});
                                            ASPxClientUtils.AttachEventToElement({2}.GetInputElement(), ""keyup"", {3});", tamanhoMaximo, labelContador.ClientInstanceName, memo.ClientInstanceName, funcao, traducao_de);

        if (calculaAltura == true)
        {
            funcaoInit += Environment.NewLine +
            string.Format(@"{0}.SetHeight(getSomatorioDasAlturasDosComponentes());", memo.ClientInstanceName);
        }

        if (tamanhoMaximo == -1)
        {
            funcaoInit = "";
            funcaoChange = "";
        }
        
        memo.ClientSideEvents.Init = "function(s,e){" + funcaoInit + " }";
        memo.ClientSideEvents.Validation = "function(s,e){" + funcaoChange + " }";
    }

    #endregion

    #region GRIDVIEW

    public string constroiInsertLayoutColunas(ASPxGridView grid, string iniciaisLista, string nomeLista)
    {
        StringBuilder comandoSql = new StringBuilder();

        comandoSql.AppendFormat(@"
BEGIN
    DECLARE @CodigoLista int
    INSERT INTO [dbo].[Lista]
	        (     [NomeLista]									, [GrupoMenu]									, [ItemMenu]
		        , [GrupoPermissao]						        , [ItemPermissao]							    , [IniciaisPermissao]
		        , [TituloLista]								    , [ComandoSelect]							
		        , [IndicaPaginacao]						        , [QuantidadeItensPaginacao]	                , [IndicaOpcaoDisponivel]			
		        , [TipoLista]									, [URL]											, [CodigoEntidade]						
		        , [CodigoModuloMenu]					        , [IndicaListaZebrada]				            , [IniciaisListaControladaSistema])
	
	        SELECT  
			      '{0}'	  		                            , 'Geral'											    , '{0}'
		        , 'Geral'									, 'Visualizar'											, 'CDIS000011'
		        , '{0}'				                        , ''
		        , 'S'									    , 50													, 'N'
		        , 'PROCESSO'								, NULL  												, {2}
		        , 'PRJ'										, 'S'													, '{1}'
		                                        
        SELECT @CodigoLista = scope_identity()", nomeLista, iniciaisLista, getInfoSistema("CodigoEntidade"));

        foreach (GridViewColumn coluna in grid.AllColumns.Where(c => c.ShowInCustomizationForm))
        {
            if (coluna is GridViewEditDataColumn)
            {
                GridViewEditDataColumn col = coluna as GridViewEditDataColumn;
                string nomeCampo = col.FieldName;
                string tituloCampo = col.Caption;
                int ordemCampo = col.VisibleIndex;
                int ordemAgrupamentoCampo = col.GroupIndex;
                string tipoCampo = "VAR";
                string formato = col.PropertiesEdit.DisplayFormatString;
                string indicaAreaFiltro = "N";
                string tipoFiltro = "N";

                bool autoFilter = col.Settings.AllowAutoFilter == DevExpress.Utils.DefaultBoolean.True || (col.Settings.AllowAutoFilter == DevExpress.Utils.DefaultBoolean.Default && grid.Settings.ShowFilterRow);
                bool headerFilter = col.Settings.AllowHeaderFilter == DevExpress.Utils.DefaultBoolean.True || (col.Settings.AllowHeaderFilter == DevExpress.Utils.DefaultBoolean.Default && grid.Settings.ShowFilterRow);

                if (autoFilter && headerFilter)
                {
                    tipoFiltro = "L";
                    indicaAreaFiltro = "S";
                }
                else if (autoFilter)
                {
                    tipoFiltro = "E";
                    indicaAreaFiltro = "S";
                }
                else if (headerFilter)
                {
                    tipoFiltro = "C";
                    indicaAreaFiltro = "S";
                }

                string indicaAgrupamento = (col.Settings.AllowGroup == DevExpress.Utils.DefaultBoolean.True || (col.Settings.AllowGroup == DevExpress.Utils.DefaultBoolean.Default && grid.SettingsBehavior.AllowGroup)) ? "S" : "N";
                string tipoTotalizador = "Nenhum";

                if (grid.TotalSummary.Contains(new ASPxSummaryItem(nomeCampo, SummaryItemType.Count)))
                    tipoTotalizador = "Contar";
                else if (grid.TotalSummary.Contains(new ASPxSummaryItem(nomeCampo, SummaryItemType.Average)))
                    tipoTotalizador = "Média";
                else if (grid.TotalSummary.Contains(new ASPxSummaryItem(nomeCampo, SummaryItemType.Sum)))
                    tipoTotalizador = "Soma";

                string indicaAreaDado = "N";
                string indicaAreaColuna = "N";
                string indicaAreaLinha = "N";
                string areaDefault = "L";
                string indicaCampoVisivel = col.Visible ? "S" : "N";
                string indicaCampoControle = col.ShowInCustomizationForm ? "N" : "S";
                string iniciaisCampoControlado = "NULL";
                string indicaLink = "NULL";
                string alinhamentoCampo = "E";

                if (col.CellStyle.HorizontalAlign == HorizontalAlign.Center)
                    alinhamentoCampo = "C";
                else if (col.CellStyle.HorizontalAlign == HorizontalAlign.Right)
                    alinhamentoCampo = "D";

                string indicaCampoHierarquia = "N";
                string larguraColuna = col.Width.IsEmpty ? "NULL" : col.Width.Value.ToString();
                string tituloColunaAgrupadora = "NULL";
                string indicaColunaFixa = col.FixedStyle == GridViewColumnFixedStyle.Left ? "S" : "N";
                #region Comando SQL

                comandoSql.AppendFormat(@"    

    INSERT INTO [dbo].[ListaCampo]([CodigoLista]
               ,[NomeCampo]
               ,[TituloCampo]
               ,[OrdemCampo]
               ,[OrdemAgrupamentoCampo]
               ,[TipoCampo]
               ,[Formato]
               ,[IndicaAreaFiltro]
               ,[TipoFiltro]
               ,[IndicaAgrupamento]
               ,[TipoTotalizador]
               ,[IndicaAreaDado]
               ,[IndicaAreaColuna]
               ,[IndicaAreaLinha]
               ,[AreaDefault]
               ,[IndicaCampoVisivel]
               ,[IndicaCampoControle]
               ,[IniciaisCampoControlado]
               ,[IndicaLink]
               ,[AlinhamentoCampo]
               ,[IndicaCampoHierarquia]
               ,[LarguraColuna]
               ,[TituloColunaAgrupadora]
               ,[IndicaColunaFixa]) VALUES (
                @CodigoLista
                , N'{0}'
                , N'{1}'
                , {2}
                , {3}
                , N'{4}'
                , '{5}'
                , N'{6}'
                , N'{7}'
                , N'{8}'
                , N'{9}'
                , N'{10}'
                , N'{11}'
                , N'{12}'
                , N'{13}'
                , N'{14}'
                , N'{15}'
                , {16}
                , {17}
                , N'{18}'
                , N'{19}'
                , {20}
                , {21}
                , N'{22}');
 ", nomeCampo
        , tituloCampo
        , ordemCampo
        , ordemAgrupamentoCampo
        , tipoCampo
        , formato.Replace("'", "\"")
        , indicaAreaFiltro
        , tipoFiltro
        , indicaAgrupamento
        , tipoTotalizador
        , indicaAreaDado
        , indicaAreaColuna
        , indicaAreaLinha
        , areaDefault
        , indicaCampoVisivel
        , indicaCampoControle
        , iniciaisCampoControlado
        , indicaLink
        , alinhamentoCampo
        , indicaCampoHierarquia
        , larguraColuna
        , tituloColunaAgrupadora
        , indicaColunaFixa
        , iniciaisLista);

                comandoSql.AppendLine();

                #endregion
            }
        }

        comandoSql.AppendLine("END");

        return comandoSql.ToString();
    }

    public void padronizaGridView(ASPxGridView gv, bool indicaPaginacao, int numeroItensPagina, bool incluiFiltroGeral)
    {
        #region VARIÁVEIS

        string nomeFonte = "Verdana";
        FontUnit tamanhoFonte = new FontUnit("8pt");
        GridViewPagerMode modoPaginacao = indicaPaginacao ? GridViewPagerMode.ShowPager : GridViewPagerMode.ShowAllRecords;

        #endregion

        #region BOTÕES

        ordenaBotoesGridView(gv);

        #endregion

        #region CARREGAMENTO

        gv.SettingsLoadingPanel.Text = "";
        gv.Images.LoadingPanel.Url = "~/imagens/carregando.gif";

        #endregion

        #region COMPORTAMENTO

        if (gv.KeyFieldName != "")
        {
            try
            {
                gv.SettingsBehavior.AllowFocusedRow = true;
                gv.KeyboardSupport = true;

            }
            catch { }
        }
        gv.SettingsResizing.ColumnResizeMode = ColumnResizeMode.NextColumn;
        gv.SettingsBehavior.AutoExpandAllGroups = true;
        gv.SettingsBehavior.EnableRowHotTrack = true;
        gv.SettingsBehavior.AllowSort = true;
        gv.Settings.ShowFilterRow = true;
        gv.SettingsBehavior.AllowGroup = false;
        gv.Settings.ShowGroupPanel = false;
        gv.EditFormLayoutProperties.EncodeHtml = false;


        #endregion

        #region ESTILOS

        gv.Styles.AlternatingRow.Enabled = DevExpress.Utils.DefaultBoolean.True;
        gv.Border.BorderStyle = BorderStyle.None;

        #endregion

        #region FILTRO

        foreach (GridViewColumn column in gv.Columns)
        {
            if (column is GridViewDataColumn)
            {
                if (((GridViewDataColumn)column).Settings.AutoFilterCondition == AutoFilterCondition.Default)
                    ((GridViewDataColumn)column).Settings.AutoFilterCondition = AutoFilterCondition.Contains;
            }
        }

        #endregion

        #region PAGINAÇÃO

        gv.SettingsPager.Mode = gv.ClientInstanceName == "gvToDoList" ? GridViewPagerMode.ShowAllRecords : modoPaginacao;
        gv.SettingsPager.PageSize = numeroItensPagina;
        gv.SettingsPager.Visible = true;
        gv.SettingsPager.AlwaysShowPager = true;

        #endregion

    }

    public void setaDefinicoesBotoesInserirExportar(ASPxMenu menu, bool podeIncluir, string funcaoJSbtnIncluir, bool mostraBtnIncluir, bool mostraBtnExportar, bool mostraBtnLayout, string iniciaisLayout, string tituloPagina, Page pagina, string tooltipBtnIncluir = "Você não possui permissão para incluir um novo registro")
    {
        #region EXPORTAÇÃO

        try
        {
            if (getInfoSistema("IDUsuarioLogado") == null)
                pagina.Response.Redirect("~/erros/erroInatividade.aspx");
        }
        catch
        {
            pagina.Response.RedirectLocation = getPathSistema() + "erros/erroInatividade.aspx";
            pagina.Response.End();
        }

        DataSet dsTemp = getParametrosSistema("exportaOLAPTodosFormatos");

        bool exportaOLAPTodosFormatos = false;

        if ((DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0])) && dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() + "" != "")
        {
            exportaOLAPTodosFormatos = (dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() == "S");
        }

        DevExpress.Web.MenuItem btnExportar = menu.Items.FindByName("btnExportar");

        btnExportar.ClientVisible = mostraBtnExportar;

        if (!exportaOLAPTodosFormatos)
        {
            btnExportar.Items.Clear();
            btnExportar.Image.Url = "~/imagens/botoes/btnExcel.png";
            btnExportar.ToolTip = Resources.traducao.exportar_para_xls;
        }
        else
        {
            btnExportar.ToolTip = Resources.traducao.exportar;
            foreach (DevExpress.Web.MenuItem item in btnExportar.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.Text))
                    item.Text = traduzExpressao(item.Text);
                if (!string.IsNullOrWhiteSpace(item.ToolTip))
                    item.ToolTip = traduzExpressao(item.ToolTip);
            }
        }

        #endregion

        #region INCLUIR

        DevExpress.Web.MenuItem btnIncluir = menu.Items.FindByName("btnIncluir");
        btnIncluir.ClientEnabled = podeIncluir;
        btnIncluir.ClientVisible = mostraBtnIncluir;
        btnIncluir.ToolTip = Resources.traducao.incluir;

        if (podeIncluir == false)
        {
            btnIncluir.Image.Url = "~/imagens/botoes/incluirRegDes.png";

            btnIncluir.ToolTip = tooltipBtnIncluir;
        }
        #endregion

        #region JS

        string clickBotaoExportar = "";

        if (exportaOLAPTodosFormatos)
            clickBotaoExportar = @"
            else if(e.item.name == 'btnExportar')
	        {
                e.processOnServer = false;		                                        
	        }";

        menu.ClientSideEvents.ItemClick =
        @"function(s, e){ 

            e.processOnServer = false;

            if(e.item.name == 'btnIncluir')
            {
                " + funcaoJSbtnIncluir + @"
            }" + clickBotaoExportar + @"		                     
	        else if(e.item.name != 'btnLayout')
	        {
                e.processOnServer = true;		                                        
	        }	
        }";

        #endregion

        #region LAYOUT

        DevExpress.Web.MenuItem btnLayout = menu.Items.FindByName("btnLayout");
        foreach (DevExpress.Web.MenuItem item in btnLayout.Items)
        {
            if (!string.IsNullOrWhiteSpace(item.Text))
                item.Text = traduzExpressao(item.Text);
            if (!string.IsNullOrWhiteSpace(item.ToolTip))
                item.ToolTip = traduzExpressao(item.ToolTip);
        }

        btnLayout.ClientVisible = mostraBtnLayout;

        if (mostraBtnLayout && !pagina.IsPostBack)
        {
            DataSet ds = getDataSet("SELECT 1 FROM Lista WHERE CodigoEntidade = " + getInfoSistema("CodigoEntidade") + " AND IniciaisListaControladaSistema = '" + iniciaisLayout + "'");

            if (ds.Tables[0].Rows.Count == 0)
            {
                int regAf = 0;

                execSQL(constroiInsertLayoutColunas((menu.Parent as GridViewHeaderTemplateContainer).Grid, iniciaisLayout, tituloPagina), ref regAf);
            }

            InitData((menu.Parent as GridViewHeaderTemplateContainer).Grid, iniciaisLayout);
        }

        #endregion
    }

    public void setaDefinicoesBotoesInserirExportarSemTemplate(ASPxMenu menu, bool podeIncluir, string funcaoJSbtnIncluir, bool mostraBtnIncluir, bool mostraBtnExportar, bool mostraBtnLayout, string iniciaisLayout, string tituloPagina, Page pagina, ASPxGridView gv)
    {
        #region EXPORTAÇÃO

        try
        {
            if (getInfoSistema("IDUsuarioLogado") == null)
                pagina.Response.Redirect("~/erros/erroInatividade.aspx");
        }
        catch
        {
            pagina.Response.RedirectLocation = getPathSistema() + "erros/erroInatividade.aspx";
            pagina.Response.End();
        }

        DataSet dsTemp = getParametrosSistema("exportaOLAPTodosFormatos");

        bool exportaOLAPTodosFormatos = false;

        if ((DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0])) && dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() + "" != "")
        {
            exportaOLAPTodosFormatos = (dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() == "S");
        }

        DevExpress.Web.MenuItem btnExportar = menu.Items.FindByName("btnExportar");

        btnExportar.ClientVisible = mostraBtnExportar;

        if (!exportaOLAPTodosFormatos)
        {
            btnExportar.Items.Clear();
            btnExportar.Image.Url = "~/imagens/botoes/btnExcel.png";
            btnExportar.ToolTip = "Exportar para XLS";
        }

        #endregion

        #region INCLUIR

        DevExpress.Web.MenuItem btnIncluir = menu.Items.FindByName("btnIncluir");
        btnIncluir.ClientEnabled = podeIncluir;

        btnIncluir.ClientVisible = mostraBtnIncluir;

        if (podeIncluir == false)
            btnIncluir.Image.Url = "~/imagens/botoes/incluirRegDes.png";

        #endregion

        #region JS

        menu.ClientSideEvents.ItemClick =
        @"function(s, e){ 

            e.processOnServer = false;

            if(e.item.name == 'btnIncluir')
            {
                " + funcaoJSbtnIncluir + @"
            }		                     
	        else if(e.item.name != 'btnLayout')
	        {
                e.processOnServer = true;		                                        
	        }	
        }";

        #endregion

        #region LAYOUT

        DevExpress.Web.MenuItem btnLayout = menu.Items.FindByName("btnLayout");

        btnLayout.ClientVisible = mostraBtnLayout;

        if (mostraBtnLayout && !pagina.IsPostBack)
        {
            DataSet ds = getDataSet("SELECT 1 FROM Lista WHERE CodigoEntidade = " + getInfoSistema("CodigoEntidade") + " AND IniciaisListaControladaSistema = '" + iniciaisLayout + "'");

            if (ds.Tables[0].Rows.Count == 0)
            {
                int regAf = 0;

                execSQL(constroiInsertLayoutColunas(gv, iniciaisLayout, tituloPagina), ref regAf);
            }

            InitData(gv, iniciaisLayout);
        }

        #endregion
    }

    public void eventoClickMenu(ASPxMenu menu, string itemClick, ASPxGridViewExporter gvExporter, string iniciaisLayoutTela)
    {
        setCodigoListaLayout(iniciaisLayoutTela);

        switch (itemClick)
        {
            case "Salvar":
                SalvarCustomizacaoLayout((menu.Parent as GridViewHeaderTemplateContainer).Grid);
                break;
            case "Restaurar":
                RestaurarLayout((menu.Parent as GridViewHeaderTemplateContainer).Grid);
                InitData((menu.Parent as GridViewHeaderTemplateContainer).Grid, iniciaisLayoutTela);
                break;
            default:
                if (menu.Parent is GridViewHeaderTemplateContainer)
                    gvExporter.GridViewID = (menu.Parent as GridViewHeaderTemplateContainer).Grid.ID;
                exportaGridView(gvExporter, itemClick);
                break;
        }
    }

    public void eventoClickMenuSemTemplate(ASPxMenu menu, string itemClick, ASPxGridViewExporter gvExporter, string iniciaisLayoutTela, ASPxGridView gv)
    {
        setCodigoListaLayout(iniciaisLayoutTela);

        switch (itemClick)
        {
            case "Salvar":
                SalvarCustomizacaoLayout(gv);
                break;
            case "Restaurar":
                RestaurarLayout(gv);
                InitData(gv, iniciaisLayoutTela);
                break;
            default:
                gvExporter.GridViewID = gv.ID;
                exportaGridView(gvExporter, itemClick);
                break;
        }
    }

    public void exportaGridView(ASPxGridViewExporter gve, string tipoExportacao)
    {
        gve.Styles.Default.Font.Name = "Verdana";
        gve.Styles.Default.Font.Size = new FontUnit("8pt");
        gve.Styles.Header.Font.Name = "Verdana";
        gve.Styles.Header.Font.Size = new FontUnit("9pt");
        gve.Styles.Header.Font.Bold = true;
        gve.Styles.GroupRow.Font.Name = "Verdana";
        gve.Styles.GroupRow.Font.Size = new FontUnit("8pt");

        gve.PreserveGroupRowStates = true;

        using (MemoryStream stream = new MemoryStream())
        {
            string dataHora = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_");

            string nomeArquivo = "";

            nomeArquivo = "Exportacao_" + dataHora;

            if (tipoExportacao == "PDF")
            {
                gve.WritePdfToResponse(nomeArquivo);
            }
            else if (tipoExportacao == "XLSX")
            {
                //Varre a primeira coluna e verifica se ela toda é vazia caso verdadeiro não exporta a linha
                bool visivel = false;
                var grid = gve.GridView;
                for (int i = 0; i < grid.VisibleRowCount; i++)
                {
                    if (!string.IsNullOrEmpty(grid.Columns[0].ToString()))
                    {
                        visivel = true;
                    }
                }
                if (!visivel)
                {
                    gve.GridView.Columns[0].Visible = false;
                }
                gve.WriteXlsxToResponse(new DevExpress.XtraPrinting.XlsxExportOptionsEx { ExportType = DevExpress.Export.ExportType.WYSIWYG });
            }
            else if (tipoExportacao == "XLS")
            {
                //Varre a primeira coluna e verifica se ela toda é vazia caso verdadeiro não exporta a linha
                bool visivel = false;
                var grid = gve.GridView;
                for (int i = 0; i < grid.VisibleRowCount; i++)
                {
                    if (!string.IsNullOrEmpty(grid.Columns[0].ToString()))
                    {
                        visivel = true;
                    }
                }
                if (!visivel)
                {
                    gve.GridView.Columns[0].Visible = false;
                }
                gve.WriteXlsToResponse(nomeArquivo, new DevExpress.XtraPrinting.XlsExportOptionsEx() { ExportType = DevExpress.Export.ExportType.WYSIWYG });
            }
            else if (tipoExportacao == "RTF")
            {
                gve.WriteRtfToResponse(nomeArquivo);
            }
            else if (tipoExportacao == "HTML")
            {

            }
            else if (tipoExportacao == "CSV")
            {
                DevExpress.XtraPrinting.CsvExportOptionsEx ceo = new DevExpress.XtraPrinting.CsvExportOptionsEx();
                ceo.Separator = ";";
                gve.WriteCsvToResponse(nomeArquivo);
            }
        }
    }

    public void ordenaBotoesGridView(ASPxGridView gv)
    {
        // A partir da versão 15.1, a definição dos botões é feita na grid
        gv.SettingsCommandButton.ClearFilterButton.RenderMode = GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.ClearFilterButton.Text = "Limpar filtros";
        gv.SettingsCommandButton.ClearFilterButton.Image.ToolTip = "Limpar filtros";
        gv.SettingsCommandButton.ClearFilterButton.Image.Url = "~/imagens/botoes/clearFilter.png";

        gv.SettingsCommandButton.NewButton.RenderMode = GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.NewButton.Text = "Incluir";
        gv.SettingsCommandButton.NewButton.Image.ToolTip = "Incluir";
        gv.SettingsCommandButton.NewButton.Image.Url = "~/imagens/botoes/incluirReg02.png";

        gv.SettingsCommandButton.EditButton.RenderMode = GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.EditButton.Text = "Alterar";
        gv.SettingsCommandButton.EditButton.Image.ToolTip = "Alterar";
        gv.SettingsCommandButton.EditButton.Image.Url = "~/imagens/botoes/editarReg02.PNG";

        gv.SettingsCommandButton.DeleteButton.RenderMode = GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.DeleteButton.Text = "Excluir";
        gv.SettingsCommandButton.DeleteButton.Image.ToolTip = "Excluir";
        gv.SettingsCommandButton.DeleteButton.Image.Url = "~/imagens/botoes/excluirReg02.PNG";

        gv.SettingsCommandButton.CancelButton.RenderMode = gv.SettingsEditing.Mode == GridViewEditingMode.Batch ? GridCommandButtonRenderMode.Link : GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.CancelButton.Text = "Cancelar";
        gv.SettingsCommandButton.CancelButton.Image.ToolTip = "Cancelar";
        gv.SettingsCommandButton.CancelButton.Image.Url = gv.SettingsEditing.Mode == GridViewEditingMode.Batch ? "" : "~/imagens/botoes/cancelar.PNG";

        gv.SettingsCommandButton.UpdateButton.RenderMode = gv.SettingsEditing.Mode == GridViewEditingMode.Batch ? GridCommandButtonRenderMode.Link : GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.UpdateButton.Text = "Salvar";
        gv.SettingsCommandButton.UpdateButton.Image.ToolTip = "Salvar";
        gv.SettingsCommandButton.UpdateButton.Image.Url = gv.SettingsEditing.Mode == GridViewEditingMode.Batch ? "" : "~/imagens/botoes/salvar.PNG";


        /*gv.SettingsCommandButton.SelectButton.ButtonType = GridCommandButtonRenderMode.Image;
        gv.SettingsCommandButton.SelectButton.Text = "Salvar";
        gv.SettingsCommandButton.SelectButton.Image.ToolTip = "Salvar";
        gv.SettingsCommandButton.SelectButton.Image.Url = "~/imagens/botoes/salvar.PNG";*/

        foreach (GridViewColumn column in gv.Columns)
        {
            if (column is GridViewCommandColumn)
            {
                // Incluído para atender a versão 15.1 que tornou obsoleto o método que utilizavamos anteriormente
                ((GridViewCommandColumn)column).ShowClearFilterButton = true;

                gv.Styles.CommandColumnItem.Paddings.PaddingLeft = 0;
                gv.Styles.CommandColumnItem.Paddings.PaddingRight = 0;
                gv.Styles.CommandColumn.Paddings.PaddingLeft = 0;
                gv.Styles.CommandColumn.Paddings.PaddingRight = 0;
                gv.Styles.CommandColumnItem.Spacing = 0;
                gv.Styles.CommandColumn.Spacing = 0;
                gv.Styles.CommandColumn.Wrap = DefaultBoolean.False;
                int quantidadeBotoes = ((GridViewCommandColumn)column).CustomButtons.Count;
                int count = 0;

                GridViewCommandColumnCustomButton[] botoes = new GridViewCommandColumnCustomButton[quantidadeBotoes];
                foreach (GridViewCommandColumnCustomButton btn in ((GridViewCommandColumn)column).CustomButtons)
                {
                    botoes[count] = btn;
                    count++;
                }

                ((GridViewCommandColumn)column).HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
                ((GridViewCommandColumn)column).HeaderStyle.Paddings.PaddingRight = 1;
                ((GridViewCommandColumn)column).HeaderStyle.Paddings.PaddingLeft = 1;

                ((GridViewCommandColumn)column).ButtonRenderMode = GridCommandButtonRenderMode.Image;
                ((GridViewCommandColumn)column).HeaderStyle.HorizontalAlign = HorizontalAlign.Center;

                //((GridViewCommandColumn)column).ClearFilterButton.Visible = true;
                //((GridViewCommandColumn)column).ClearFilterButton.Text = "Limpar filtros";
                //((GridViewCommandColumn)column).ClearFilterButton.Image.ToolTip = "Limpar filtros";
                //((GridViewCommandColumn)column).ClearFilterButton.Image.Url = "~/imagens/botoes/clearFilter.png";

                //((GridViewCommandColumn)column).EditButton.Text = "Alterar";
                //((GridViewCommandColumn)column).EditButton.Image.ToolTip = "Alterar";

                //((GridViewCommandColumn)column).DeleteButton.Text = "Excluir";
                //((GridViewCommandColumn)column).DeleteButton.Image.ToolTip = "Excluir";

                ((GridViewCommandColumn)column).CustomButtons.Clear();

                bool btnAltOK = false, btnExcOK = false;

                foreach (GridViewCommandColumnCustomButton btn in botoes)
                {
                    if (btn.Image.Url.Contains("editarReg"))
                    {
                        btn.Image.ToolTip = "Alterar";
                        btn.Text = "Alterar";
                        ((GridViewCommandColumn)column).CustomButtons.Insert(0, btn);

                        btnAltOK = true;
                    }
                    else if (btn.Image.Url.Contains("excluirReg"))
                    {
                        int posicao = 0;

                        posicao = btnAltOK ? (posicao + 1) : posicao;
                        btn.Image.ToolTip = "Excluir";
                        btn.Text = "Excluir";

                        ((GridViewCommandColumn)column).CustomButtons.Insert(posicao, btn);

                        btnExcOK = true;
                    }
                    else if (btn.Image.Url.Contains("pFormulario"))
                    {
                        int posicao = 0;

                        posicao = btnAltOK ? (posicao + 1) : posicao;

                        posicao = btnExcOK ? (posicao + 1) : posicao;
                        btn.Image.ToolTip = "Visualizar Detalhes";
                        btn.Text = "Visualizar Detalhes";

                        ((GridViewCommandColumn)column).CustomButtons.Insert(posicao, btn);
                    }
                    else
                    {
                        ((GridViewCommandColumn)column).CustomButtons.Add(btn);
                    }
                }
            }
            else
            {

                column.ExportWidth = column.Width.IsEmpty ? 400 : (int)(column.Width.Value + 40);

                if (column is GridViewEditDataColumn)
                {
                    //((GridViewEditDataColumn)column).Settings.AllowHeaderFilter = DefaultBoolean.False;

                    if (column is GridViewDataComboBoxColumn)
                    {
                        ((GridViewDataComboBoxColumn)column).PropertiesComboBox.IncrementalFilteringMode = IncrementalFilteringMode.Contains;
                        ((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataComboBoxColumn)column).PropertiesComboBox.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                    else if (column is GridViewDataTextColumn)
                    {
                        ((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataTextColumn)column).PropertiesTextEdit.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                    else if (column is GridViewDataDateColumn)
                    {
                        ((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataDateColumn)column).PropertiesDateEdit.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                    else if (column is GridViewDataMemoColumn)
                    {
                        ((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataMemoColumn)column).PropertiesMemoEdit.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                    else if (column is GridViewDataSpinEditColumn)
                    {
                        ((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataSpinEditColumn)column).PropertiesSpinEdit.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                    else if (column is GridViewDataCheckColumn)
                    {
                        ((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.Display = Display.Dynamic;
                        ((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.ErrorDisplayMode = ErrorDisplayMode.ImageWithTooltip;
                        ((GridViewDataCheckColumn)column).PropertiesCheckEdit.ValidationSettings.RequiredField.ErrorText = "Campo obrigatório!";
                    }
                }
                else if (column is GridViewDataColumn)
                {
                    ((GridViewDataColumn)column).Settings.AllowHeaderFilter = DefaultBoolean.False;
                }
            }

        }
    }

    public DataTable getDataTableFiltrado(ASPxGridView gv, DataSet ds, string expressao)
    {
        var searchText = "";

        if (gv.FindTitleTemplateControl("btnSearch_" + gv.ClientInstanceName) != null)
            searchText = (gv.FindTitleTemplateControl("btnSearch_" + gv.ClientInstanceName) as ASPxButtonEdit).Text;

        DataRow[] drs = ds.Tables[0].Select(string.Format(expressao, searchText));

        DataTable dt = ds.Tables[0].Clone();

        foreach (DataRow dr in drs)
            dt.ImportRow(dr);

        return dt;
    }

    class TitleTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            GridViewTitleTemplateContainer gridContainer = (GridViewTitleTemplateContainer)container;
            ASPxButtonEdit btnFiltro = new ASPxButtonEdit();
            string nomeFonte = "Verdana";
            FontUnit tamanhoFonte = new FontUnit("8pt");

            btnFiltro.Border.BorderColor = Color.Gray;
            btnFiltro.ButtonStyle.Border.BorderColor = Color.Gray;
            btnFiltro.ButtonStyle.BackColor = Color.White;
            btnFiltro.ButtonStyle.BackgroundImage.ImageUrl = "~/imagens.vazioPequeno.gif";
            btnFiltro.ID = "btnSearch_" + gridContainer.Grid.ClientInstanceName;
            btnFiltro.ClientInstanceName = "btnSearch_" + gridContainer.Grid.ClientInstanceName;
            btnFiltro.Font.Name = nomeFonte;
            btnFiltro.Font.Size = tamanhoFonte;
            btnFiltro.NullText = "Procurar...";
            btnFiltro.Width = 350;
            btnFiltro.Height = 20;
            btnFiltro.Buttons.Clear();
            btnFiltro.ButtonStyle.Paddings.Padding = 0;
            btnFiltro.Paddings.Padding = 3;
            btnFiltro.Focus();

            #region Client Functions

            btnFiltro.ClientSideEvents.TextChanged = "function(s, e) { " + gridContainer.Grid.ClientInstanceName + ".PerformCallback(); }";
            btnFiltro.ClientSideEvents.KeyPress = @"function(s, e) { 
                                                                    e = e.htmlEvent;
                                                                    if (e.keyCode === 13) {
                                                                        // prevent default browser form submission
                                                                        if (e.preventDefault)
                                                                            e.preventDefault();
                                                                        else
                                                                            e.returnValue = false;
                                                                    } 
                                                                }";

            #endregion

            EditButton btn = new EditButton();
            btn.Image.Url = "~/imagens/searchBox.png";
            btn.Width = 25;
            btnFiltro.Buttons.Add(btn);
            container.Controls.Add(btnFiltro);
        }
    }

    public class EditFormTemplate : ITemplate
    {
        public void InstantiateIn(Control container)
        {
            GridViewEditFormTemplateContainer gridContainer = (GridViewEditFormTemplateContainer)container;
            Table table = CreateHtmlTable();
            container.Controls.Add(table);

            ASPxGridViewTemplateReplacement tmp = new ASPxGridViewTemplateReplacement();
            tmp.ReplacementType = GridViewTemplateReplacementType.EditFormEditors;
            table.Style.Add(HtmlTextWriterStyle.Padding, "0px");
            table.Rows[0].Cells[0].Controls.Add(tmp);
            table.Attributes.Add("cellspacing", "0");
            table.Attributes.Add("cellpadding", "0");

            ASPxButton btnSalvar = new ASPxButton();
            string nomeFonte = "Roboto Regular";
            FontUnit tamanhoFonte = new FontUnit("14px");

            btnSalvar.ID = "CustomBtnSalvarForm";
            btnSalvar.Font.Name = nomeFonte;
            btnSalvar.Font.Size = tamanhoFonte;
            btnSalvar.Width = 90;
            btnSalvar.Paddings.Padding = 1;
            btnSalvar.Text = Resources.traducao.salvar;
            btnSalvar.AutoPostBack = false;
            btnSalvar.ValidationGroup = "MKE";
            btnSalvar.Style.Add("text-transform", "capitalize");

            ASPxButton btnFechar = new ASPxButton();

            btnFechar.Font.Name = nomeFonte;
            btnFechar.Font.Size = tamanhoFonte;
            btnFechar.Width = 90;
            btnFechar.Paddings.Padding = 1;
            btnFechar.Text = Resources.traducao.fechar;
            btnFechar.AutoPostBack = false;
            btnFechar.Style.Add("text-transform", "capitalize");


            dados cDados = CdadosUtil.GetCdados(null);

            cDados.aplicaEstiloVisual(btnSalvar);
            cDados.aplicaEstiloVisual(btnFechar);

            #region Client Functions

            string funcaoSalvar = @"
            if(ASPxClientEdit.ValidateGroup('MKE', true))
            {
                " + gridContainer.Grid.ClientInstanceName + @".UpdateEdit();
            }";

            btnSalvar.ClientSideEvents.Click = "function(s, e) { " + funcaoSalvar + " }";
            btnFechar.ClientSideEvents.Click = "function(s, e) { " + gridContainer.Grid.ClientInstanceName + ".CancelEdit(); }";

            #endregion

            Table tableBotoes = new Table();
            tableBotoes.Rows.Add(new TableRow());
            tableBotoes.Rows[0].Cells.AddRange(new TableCell[] { new TableCell(),
                                                                 new TableCell()});

            tableBotoes.Rows[0].Cells[0].Controls.Add(btnSalvar);
            tableBotoes.Rows[0].Cells[1].Controls.Add(btnFechar);
            tableBotoes.Rows[0].Cells[0].Style.Add("padding-right", "10px");

            table.Rows[1].Cells[0].HorizontalAlign = HorizontalAlign.Right;
            table.Rows[1].Cells[0].Controls.Add(tableBotoes);
            table.CellPadding = 0;
            table.CellSpacing = 0;
            tableBotoes.CellPadding = 0;
            tableBotoes.CellSpacing = 0;

        }

        Table CreateHtmlTable()
        {
            Table table = new Table();
            table.Width = new Unit("100%");
            table.Rows.Add(new TableRow());
            table.Rows[0].Cells.AddRange(new TableCell[] { new TableCell() });
            table.Rows.Add(new TableRow());
            table.Rows[1].Cells.AddRange(new TableCell[] { new TableCell() });
            return table;
        }
    }

    #endregion

    #region OLAP

    public void eventoClickMenuOLAP(ASPxMenu menu, string itemClick, DevExpress.Web.ASPxPivotGrid.ASPxPivotGridExporter pge, Page pagina, string iniciaisLayoutTela, ASPxPivotGrid gv)
    {
        setCodigoListaLayout(iniciaisLayoutTela);

        switch (itemClick)
        {
            case "Salvar":
                SalvarCustomizacaoLayoutOLAP(gv);
                break;
            case "Restaurar":
                RestaurarLayoutOLAP(gv);
                InitDataOLAP(gv, iniciaisLayoutTela);
                break;
            default:
                pge.ASPxPivotGridID = gv.ID;
                exportaOlap(pge, itemClick, pagina);
                break;
        }
    }

    public void setaDefinicoesBotoesExportarLayoutOLAP(ASPxMenu menu, bool mostraBtnExportar, bool mostraBtnLayout, string iniciaisLayout, string tituloPagina, Page pagina, DevExpress.Web.ASPxPivotGrid.ASPxPivotGrid pvg)
    {
        #region EXPORTAÇÃO

        try
        {
            if (getInfoSistema("IDUsuarioLogado") == null)
                pagina.Response.Redirect("~/erros/erroInatividade.aspx");
        }
        catch
        {
            pagina.Response.RedirectLocation = getPathSistema() + "erros/erroInatividade.aspx";
            pagina.Response.End();
        }

        DataSet dsTemp = getParametrosSistema("exportaOLAPTodosFormatos");

        bool exportaOLAPTodosFormatos = false;

        if ((DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0])) && dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() + "" != "")
        {
            exportaOLAPTodosFormatos = (dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() == "S");
        }

        DevExpress.Web.MenuItem btnExportar = menu.Items.FindByName("btnExportar");

        btnExportar.ClientVisible = mostraBtnExportar;

        if (!exportaOLAPTodosFormatos)
        {
            btnExportar.Items.Clear();
            btnExportar.Image.Url = "~/imagens/botoes/btnExcel.png";
            btnExportar.ToolTip = "Exportar para XLS";
        }

        #endregion

        DevExpress.Web.MenuItem btnIncluir = menu.Items.FindByName("btnIncluir");

        if (btnIncluir != null)
        {
            btnIncluir.ClientEnabled = false;
            btnIncluir.ClientVisible = false;
        }

        #region LAYOUT

        DevExpress.Web.MenuItem btnLayout = menu.Items.FindByName("btnLayout");

        btnLayout.ClientVisible = mostraBtnLayout;

        if (mostraBtnLayout && !pagina.IsPostBack)
        {
            DataSet ds = getDataSet("SELECT 1 FROM Lista WHERE CodigoEntidade = " + getInfoSistema("CodigoEntidade") + " AND IniciaisListaControladaSistema = '" + iniciaisLayout + "'");

            if (ds.Tables[0].Rows.Count == 0)
            {
                int regAf = 0;

                execSQL(constroiInsertLayoutColunasOLAP(pvg, iniciaisLayout, tituloPagina), ref regAf);
            }

            InitDataOLAP(pvg, iniciaisLayout);
        }

        #endregion
    }

    public string constroiInsertLayoutColunasOLAP(ASPxPivotGrid grid, string iniciaisLista, string nomeLista)
    {
        StringBuilder comandoSql = new StringBuilder();

        comandoSql.AppendFormat(@"INSERT INTO [dbo].[Lista]
	                                        (  [NomeLista]									, [GrupoMenu]									, [ItemMenu]
		                                     , [GrupoPermissao]						        , [ItemPermissao]							    , [IniciaisPermissao]
		                                     , [TituloLista]								, [ComandoSelect]							
		                                     , [IndicaPaginacao]						    , [QuantidadeItensPaginacao]	                , [IndicaOpcaoDisponivel]			
		                                     , [TipoLista]									, [URL]											, [CodigoEntidade]						
		                                     , [CodigoModuloMenu]					        , [IndicaListaZebrada]				            , [IniciaisListaControladaSistema])
	
	                                        SELECT  
			                                      '{0}'	  		                            , 'Geral'												, '{0}'
		                                        , 'Geral'									, 'Visualizar'											, 'CDIS000011'
		                                        , '{0}'				                        , ''
		                                        , 'S'									    , 50													, 'N'
		                                        , 'OLAP'								    , NULL  												, {2}
		                                        ,  NULL										, 'S'													, '{1}'
		                                        ", nomeLista, iniciaisLista, getInfoSistema("CodigoEntidade"));

        foreach (DevExpress.Web.ASPxPivotGrid.PivotGridField col in grid.Fields.OfType<DevExpress.Web.ASPxPivotGrid.PivotGridField>().Where(f => f.CanShowInCustomizationForm))
        {

            string nomeCampo = col.FieldName;
            string tituloCampo = col.Caption;
            int ordemCampo = col.AreaIndex;
            int ordemAgrupamentoCampo = -1;
            string tipoCampo = "";

            if (col.CellFormat.FormatType == FormatType.Numeric)
                tipoCampo = "NUM";
            else if (col.CellFormat.FormatType == FormatType.DateTime)
                tipoCampo = "DAT";
            else if (col.CellFormat.FormatType == FormatType.Custom)
                tipoCampo = "VAR";

            string formato = col.CellFormat.FormatString;
            string indicaAreaFiltro = "N";
            string tipoFiltro = "N";
            string indicaAreaDado = "N";
            string indicaAreaColuna = "N";
            string indicaAreaLinha = "N";
            string areaDefault = "L";

            if (col.Area == PivotArea.DataArea)
                areaDefault = "D";
            else if (col.Area == PivotArea.ColumnArea)
                areaDefault = "C";
            else if (col.Area == PivotArea.FilterArea)
                areaDefault = "F";

            if (col.IsAreaAllowed(PivotArea.FilterArea))
            {
                tipoFiltro = "E";
                indicaAreaFiltro = "S";
            }

            if (col.IsAreaAllowed(PivotArea.DataArea))
            {
                indicaAreaDado = "S";
            }

            if (col.IsAreaAllowed(PivotArea.ColumnArea))
            {
                indicaAreaColuna = "S";
            }

            if (col.IsAreaAllowed(PivotArea.RowArea))
            {
                indicaAreaLinha = "S";
            }

            string indicaAgrupamento = "N";
            string tipoTotalizador = "Soma";

            if (col.Options.ShowCustomTotals == false && col.Options.ShowGrandTotal == false && col.Options.ShowTotals == false)
                tipoTotalizador = "Nenhum";

            string indicaCampoVisivel = col.Visible ? "S" : "N";
            string indicaCampoControle = "N";
            string iniciaisCampoControlado = "NULL";
            string indicaLink = "NULL";
            string alinhamentoCampo = "E";

            if (col.CellStyle.HorizontalAlign == HorizontalAlign.Center)
                alinhamentoCampo = "C";
            else if (col.CellStyle.HorizontalAlign == HorizontalAlign.Right)
                alinhamentoCampo = "D";

            string indicaCampoHierarquia = "N";
            string larguraColuna = "NULL";
            string tituloColunaAgrupadora = "NULL";
            #region Comando SQL

            comandoSql.AppendFormat(@"INSERT INTO  [dbo].[ListaCampo]
           ([CodigoLista]
           ,[NomeCampo]
           ,[TituloCampo]
           ,[OrdemCampo]
           ,[OrdemAgrupamentoCampo]
           ,[TipoCampo]
           ,[Formato]
           ,[IndicaAreaFiltro]
           ,[TipoFiltro]
           ,[IndicaAgrupamento]
           ,[TipoTotalizador]
           ,[IndicaAreaDado]
           ,[IndicaAreaColuna]
           ,[IndicaAreaLinha]
           ,[AreaDefault]
           ,[IndicaCampoVisivel]
           ,[IndicaCampoControle]
           ,[IniciaisCampoControlado]
           ,[IndicaLink]
           ,[AlinhamentoCampo]
           ,[IndicaCampoHierarquia]
           ,[LarguraColuna]
           ,[TituloColunaAgrupadora])
            SELECT l.[CodigoLista], N'{0}', N'{1}', {2}, {3}, N'{4}', '{5}', N'{6}', N'{7}', N'{8}', N'{9}', N'{10}', N'{11}', N'{12}', N'{13}', N'{14}', N'{15}', {16}, {17}, N'{18}', N'{19}', {20}, {21} FROM [dbo].[Lista] AS [l]  WHERE l.[IniciaisListaControladaSistema] = '{22}' AND NOT EXISTS( SELECT TOP 1 1 FROM [dbo].[ListaCampo] AS lc  WHERE lc.[CodigoLista] = l.[CodigoLista] AND lc.[NomeCampo] =	'{0}');
 ", nomeCampo
    , tituloCampo
    , ordemCampo
    , ordemAgrupamentoCampo
    , tipoCampo
    , formato.Replace("'", "\"")
    , indicaAreaFiltro
    , tipoFiltro
    , indicaAgrupamento
    , tipoTotalizador
    , indicaAreaDado
    , indicaAreaColuna
    , indicaAreaLinha
    , areaDefault
    , indicaCampoVisivel
    , indicaCampoControle
    , iniciaisCampoControlado
    , indicaLink
    , alinhamentoCampo
    , indicaCampoHierarquia
    , larguraColuna
    , tituloColunaAgrupadora
    , iniciaisLista);

            comandoSql.AppendLine();

            #endregion

        }

        return comandoSql.ToString();
    }

    public void exportaOlap(ASPxPivotGridExporter pge, string tipoExportacao, Page pagina)
    {
        string dataHora = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_");

        string nomeArquivo = "";

        nomeArquivo = "Exportacao_" + dataHora;

        if (tipoExportacao == "PDF")
        {
            pge.ExportPdfToResponse(nomeArquivo);
        }
        else if (tipoExportacao == "XLS" || tipoExportacao == "XLSX")
        {
            DevExpress.XtraPrinting.XlsxExportOptionsEx ex = new DevExpress.XtraPrinting.XlsxExportOptionsEx();
            ex.ExportType = DevExpress.Export.ExportType.WYSIWYG;
            pge.ExportXlsxToResponse(nomeArquivo, ex);
        }
        else if (tipoExportacao == "RTF")
        {
            pge.ExportRtfToResponse(nomeArquivo);
        }
        else if (tipoExportacao == "HTML")
        {

        }
        else if (tipoExportacao == "CSV")
        {
            pge.ExportCsvToResponse(nomeArquivo);
        }
    }

    public void StartProcess(string path)
    {
        Process process = new Process();
        try
        {
            process.StartInfo.FileName = path;
            process.Start();
            process.WaitForInputIdle();
        }
        catch { }
    }

    public void configuraPainelBotoesOLAP(HtmlTable tbBotoes)
    {
        string estiloFooter = "dxpgControl dxpgColumnTotalFieldValue dxpgRowGrandTotalFieldValue";

        string cssPostfix = "", cssPath = "";

        var idVisual = getInfoSistema("IDEstiloVisual");
        if (idVisual == null)
            getVisual("Office2003Blue", ref cssPath, ref cssPostfix);
        else
            getVisual(getInfoSistema("IDEstiloVisual").ToString(), ref cssPath, ref cssPostfix);


        if (cssPostfix != "")
            estiloFooter = "dxpgControl_" + cssPostfix + " dxpgColumnTotalFieldValue_" + cssPostfix + " dxpgRowGrandTotalFieldValue_" + cssPostfix;

        tbBotoes.Attributes.Add("class", estiloFooter);

        tbBotoes.Style.Add("padding", "3px");

        tbBotoes.Style.Add("border-collapse", "collapse");

        tbBotoes.Style.Add("border", "solid");

        tbBotoes.Style.Add("border", "none");
    }

    #region GRAVAÇÃO e RECUPERAÇÃO DE LAYOUT

    public void InitDataOLAP(ASPxPivotGrid gvDados, string iniciaisLista)
    {
        dsLP = new DsListaProcessos();

        setCodigoListaLayout(iniciaisLista);

        string comandoPreencheLista;
        string comandoPreencheListaCampo;
        string comandoPreencheListaFluxo;

        #region Comandos SQL

        comandoPreencheLista = string.Format(@"
 SELECT l.CodigoLista, 
		NomeLista, 
		GrupoMenu, 
		ItemMenu, 
		GrupoPermissao, 
		ItemPermissao, 
		IniciaisPermissao, 
		TituloLista, 
		ComandoSelect, 
		IndicaPaginacao, 
		ISNULL(lu.QuantidadeItensPaginacao, l.QuantidadeItensPaginacao) AS QuantidadeItensPaginacao, 
		IndicaOpcaoDisponivel, 
		TipoLista, 
		URL, 
		CodigoEntidade,
        CodigoModuloMenu,
        IndicaListaZebrada,
        lu.FiltroAplicado
   FROM Lista l left join ListaUsuario lu ON (lu.CodigoLista = l.CodigoLista AND lu.CodigoUsuario = {0}) 
  WHERE (l.CodigoLista = @CodigoLista)", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaCampo = string.Format(@"
 SELECT lc.CodigoCampo,
        lc.CodigoLista, 
        lc.NomeCampo, 
        lc.TituloCampo, 
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo) AS OrdemCampo, 
        ISNULL(lcu.OrdemAgrupamentoCampo, lc.OrdemAgrupamentoCampo) AS OrdemAgrupamentoCampo,
        lc.TipoCampo, 
        lc.Formato, 
        lc.IndicaAreaFiltro, 
        lc.TipoFiltro, 
        lc.IndicaAgrupamento, 
        lc.TipoTotalizador, 
        lc.IndicaAreaDado, 
        lc.IndicaAreaColuna, 
        lc.IndicaAreaLinha, 
        ISNULL(lcu.AreaDefault, lc.AreaDefault) AS AreaDefault, 
        ISNULL(lcu.IndicaCampoVisivel, lc.IndicaCampoVisivel) AS IndicaCampoVisivel, 
        lc.IndicaCampoControle,
        lc.IniciaisCampoControlado,
        lc.IndicaLink,
        (CASE WHEN lcu.CodigoCampo IS NOT NULL THEN 'S' ELSE 'N' END) AS IndicaCampoCustumizado,
        lc.AlinhamentoCampo,
        lc.IndicaCampoHierarquia,
        ISNULL(lcu.LarguraColuna, lc.LarguraColuna) AS LarguraColuna
   FROM ListaCampo AS lc LEFT JOIN
		ListaUsuario lu ON lu.CodigoLista = lc.CodigoLista AND 
						   lu.CodigoUsuario = {0} LEFT JOIN
        ListaCampoUsuario lcu ON lc.CodigoCampo = lcu.CodigoCampo AND 
                                 lcu.CodigoListaUsuario = lu.CodigoListaUsuario
  WHERE (lc.CodigoLista = @CodigoLista)
  ORDER BY
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo)", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaFluxo = @"
 SELECT CodigoLista, 
		CodigoFluxo, 
		TituloMenu
   FROM ListaFluxo
  WHERE (CodigoLista = @CodigoLista)";

        #endregion

        FillData(dsLP.Lista, comandoPreencheLista);
        FillData(dsLP.ListaCampo, comandoPreencheListaCampo);
        FillData(dsLP.ListaFluxo, comandoPreencheListaFluxo);

        DsListaProcessos.ListaRow item = dsLP.Lista.Single();

        SetGridSettingsOLAP(item, gvDados);
    }

    private void SetGridSettingsOLAP(DsListaProcessos.ListaRow item, ASPxPivotGrid pgDados)
    {
        bool possuiPaginacao = VerificaVerdadeiroOuFalso(item.IndicaPaginacao);

        pgDados.OptionsPager.Visible = possuiPaginacao;

        if (possuiPaginacao)
            pgDados.OptionsPager.RowsPerPage = item.QuantidadeItensPaginacao;

        foreach (var campo in item.GetListaCampoRows())
            AddPivotGridFileds(campo, pgDados);

        if (item.FiltroAplicado != null && item.FiltroAplicado.Contains("¥¥") && !IsPostBack)
        {
            aplicaFiltroColunas(item.FiltroAplicado, pgDados);
        }
    }

    private void aplicaFiltroColunas(string filtroExp, ASPxPivotGrid pgDados)
    {
        string[] campos = filtroExp.Split(new string[] { "¥¥" }, StringSplitOptions.None);

        foreach (string campo in campos)
        {
            if (campo != "")
            {
                string fieldName = campo.Split(new string[] { "$$" }, StringSplitOptions.None)[0];
                string[] valores = campo.Substring(campo.IndexOf("$$") + 2).Split(new string[] { "##" }, StringSplitOptions.None);

                // Locks the control to prevent excessive updates when multiple properties are modified.
                pgDados.BeginUpdate();
                try
                {
                    pgDados.Fields[fieldName].FilterValues.Clear();

                    foreach (string valor in valores)
                        pgDados.Fields[fieldName].FilterValues.Add(valor);

                    pgDados.Fields[fieldName].FilterValues.FilterType = DevExpress.XtraPivotGrid.PivotFilterType.Excluded;
                }
                finally
                {
                    // Unlocks the control.
                    pgDados.EndUpdate();
                }
            }
        }
    }

    private void AddPivotGridFileds(DsListaProcessos.ListaCampoRow campo, ASPxPivotGrid pgDados)
    {
        DevExpress.Web.ASPxPivotGrid.PivotGridField field = (DevExpress.Web.ASPxPivotGrid.PivotGridField)pgDados.Fields[campo.NomeCampo];

        if (field == null)
            return;

        field.FilterValues.Clear();

        if (VerificaVerdadeiroOuFalso(campo.IndicaCampoControle))
        {
            field.Visible = false;
            field.Options.ShowInCustomizationForm = false;
            string iniciais = campo.IniciaisCampoControlado;
            if (!string.IsNullOrEmpty(iniciais))
                field.Name = string.Format("fieldCC_{0}", iniciais);
        }
        else
        {
            if (campo.TipoCampo.ToUpper().Equals("BLT"))
            {
                field.CellFormat.FormatString = "<img alt='' src='../../imagens/{0}Menor.gif' />";
                field.CellFormat.FormatType = FormatType.Custom;
                field.ValueFormat.FormatString = "<img alt='' src='../../imagens/{0}Menor.gif' />";
                field.ValueFormat.FormatType = FormatType.Custom;
                field.CellStyle.HorizontalAlign = HorizontalAlign.Center;
                field.ValueStyle.HorizontalAlign = HorizontalAlign.Center;
                field.Name = string.Format("fieldBLT_{0}#{1}"
                    , campo.NomeCampo, campo.CodigoCampo);
                field.SummaryType = PivotSummaryType.Custom;
            }
            else
            {
                field.Name = string.Format("field#{0}", campo.CodigoCampo);
                //field.UseNativeFormat = DefaultBoolean.True;
            }
            field.Area = GetPivotArea(campo.AreaDefault);
            field.AreaIndex = campo.OrdemCampo;
            field.AllowedAreas = GetAllowedAreas(campo);
            field.Caption = campo.TituloCampo;
            if (!string.IsNullOrEmpty(campo.Formato))
            {
                field.CellFormat.FormatType = GetFormatType(campo.TipoCampo);
                field.CellFormat.FormatString = campo.Formato;
                //Incluído por Amauri em 07/11/2013 para resolver a formatação de colunas Monetário na área de linhas
                field.ValueFormat.FormatType = GetFormatType(campo.TipoCampo);
                field.ValueFormat.FormatString = campo.Formato;
            }
            if (campo.TipoTotalizador.ToLower() == "nenhum")
            {
                field.Options.ShowCustomTotals = false;
                field.Options.ShowGrandTotal = false;
                field.Options.ShowTotals = false;
            }
            field.Visible = VerificaVerdadeiroOuFalso(campo.IndicaCampoVisivel);
        }
    }

    private static FormatType GetFormatType(string tipoCampo)
    {
        /*
		 * - NUM
		 * - TXT
		 * - DAT
		 * - VAR
		 * - MON (Valor monetário)
		 * - PER (Valor percentual)
		 */
        switch (tipoCampo.ToUpper())
        {
            case "NUM":
            case "MON":
            case "PER":
                return FormatType.Numeric;
            case "DAT":
                return FormatType.DateTime;
            case "VAR":
                return FormatType.Custom;
            default:
                return FormatType.None;
        }
    }

    private PivotArea GetPivotArea(string strPivotArea)
    {
        switch (strPivotArea.ToUpper())
        {
            case "D":
                return PivotArea.DataArea;
            case "L":
                return PivotArea.RowArea;
            case "C":
                return PivotArea.ColumnArea;
            case "F":
                return PivotArea.FilterArea;
            default:
                throw new ArgumentException("A área informada não é válida.");
        }
    }

    private PivotGridAllowedAreas GetAllowedAreas(DsListaProcessos.ListaCampoRow campo)
    {
        bool indicaAreaDados = VerificaVerdadeiroOuFalso(campo.IndicaAreaDado);
        bool indicaAreaLinha = VerificaVerdadeiroOuFalso(campo.IndicaAreaLinha);
        bool indicaAreaColuna = VerificaVerdadeiroOuFalso(campo.IndicaAreaColuna);
        bool indicaAreaFiltro = VerificaVerdadeiroOuFalso(campo.IndicaAreaFiltro);
        int valorAreaDado = (int)PivotGridAllowedAreas.DataArea;
        int valorAreaLinha = (int)PivotGridAllowedAreas.RowArea;
        int valorAreaColuna = (int)PivotGridAllowedAreas.ColumnArea;
        int valorAreaFiltro = (int)PivotGridAllowedAreas.FilterArea;
        const int CONST_Zero = 0;
        int valorAreasPermitidas =
            (indicaAreaDados ? valorAreaDado : CONST_Zero) |
            (indicaAreaLinha ? valorAreaLinha : CONST_Zero) |
            (indicaAreaColuna ? valorAreaColuna : CONST_Zero) |
            (indicaAreaFiltro ? valorAreaFiltro : CONST_Zero);
        PivotGridAllowedAreas allowedAreas = (PivotGridAllowedAreas)valorAreasPermitidas;
        return allowedAreas;
    }

    public void SalvarCustomizacaoLayoutOLAP(ASPxPivotGrid pgDados)
    {
        string filter = getFilter(pgDados);

        StringBuilder comandoSql = new StringBuilder(@"
DECLARE @CodigoLista Int,
        @CodigoUsuario INT,
        @CodigoCampo INT,
        @OrdemCampo SMALLINT,
        @OrdemAgrupamentoCampo SMALLINT,
        @AreaDefault CHAR(1),
        @IndicaCampoVisivel CHAR(1),
        @FiltroAplicado VARCHAR(max),
        @CodigoListaUsuario BIGINT");
        comandoSql.AppendLine();
        comandoSql.AppendFormat(@"
	SET @CodigoLista = {0}
	SET @CodigoUsuario = {1}
	SET @FiltroAplicado = '{2}'
            
 SELECT TOP 1 @CodigoListaUsuario = CodigoListaUsuario 
   FROM ListaUsuario AS lu 
  WHERE lu.CodigoLista = @CodigoLista 
    AND lu.CodigoUsuario = @CodigoUsuario

IF @CodigoListaUsuario IS NOT NULL
BEGIN
     UPDATE ListaUsuario
        SET FiltroAplicado = @FiltroAplicado
      WHERE CodigoListaUsuario = @CodigoListaUsuario
END
ELSE
BEGIN
     INSERT INTO ListaUsuario(CodigoUsuario, CodigoLista, FiltroAplicado, IndicaListaPadrao) VALUES (@CodigoUsuario, @CodigoLista, @FiltroAplicado, 'S')
   
        SET @CodigoListaUsuario = SCOPE_IDENTITY()
END"
     , codigoLista, getInfoSistema("IDUsuarioLogado"), filter.Replace("'", "''"));

        foreach (DevExpress.Web.ASPxPivotGrid.PivotGridField field in pgDados.Fields.OfType<DevExpress.Web.ASPxPivotGrid.PivotGridField>().Where(f => f.CanShowInCustomizationForm))
        {
            int ordemCampo = field.AreaIndex;
            int ordemAgrupamentoCampo = -1;
            string areaDefault = ObtemInicialArea(field.Area);
            string indicaCampoVisivel = field.Visible ? "S" : "N";
            int codigoCampo = ObtemCodigoCampoOLAP(field);

            if (codigoCampo > 0)
            {
                #region Comando SQL

                comandoSql.AppendFormat(@"
    SET @CodigoUsuario = {0}
    SET @CodigoCampo = {1}
    SET @OrdemCampo = {2}
    SET @OrdemAgrupamentoCampo = {3}
    SET @AreaDefault = '{4}'
    SET @IndicaCampoVisivel = '{5}'

 IF EXISTS(SELECT 1 FROM ListaCampoUsuario AS lcu WHERE lcu.CodigoCampo = @CodigoCampo AND lcu.CodigoListaUsuario = @CodigoListaUsuario)
BEGIN
     UPDATE ListaCampoUsuario
        SET OrdemCampo = @OrdemCampo,
            OrdemAgrupamentoCampo = @OrdemAgrupamentoCampo,
            AreaDefault = @AreaDefault,
            IndicaCampoVisivel = @IndicaCampoVisivel
      WHERE CodigoCampo = @CodigoCampo
        AND CodigoListaUsuario = @CodigoListaUsuario
END
ELSE
BEGIN
     INSERT INTO ListaCampoUsuario(
            CodigoCampo,
            CodigoListaUsuario,
            OrdemCampo,
            OrdemAgrupamentoCampo,
            AreaDefault,
            IndicaCampoVisivel)
     VALUES(
            @CodigoCampo,
            @CodigoListaUsuario,
            @OrdemCampo,
            @OrdemAgrupamentoCampo,
            @AreaDefault,
            @IndicaCampoVisivel)
END",
     getInfoSistema("IDUsuarioLogado"),
     codigoCampo,
     ordemCampo,
     ordemAgrupamentoCampo,
     areaDefault,
     indicaCampoVisivel);
                comandoSql.AppendLine();

                #endregion
            }
        }
        int registrosAfetados = 0;
        execSQL(comandoSql.ToString(), ref registrosAfetados);
    }

    public string getFilter(ASPxPivotGrid pgDados)
    {
        string filterExp = "";

        foreach (DevExpress.Web.ASPxPivotGrid.PivotGridField field in pgDados.Fields)
        {
            if (field.FilterValues.Count > 0)
            {
                filterExp += field.FieldName + "$$";

                foreach (object valores in field.FilterValues.ValuesExcluded)
                {
                    if (valores != null && valores.ToString() != "")
                        filterExp += valores + "##";
                }

                filterExp += "¥¥";
            }
        }

        return filterExp;
    }

    private string ObtemInicialArea(PivotArea area)
    {
        switch (area)
        {
            case PivotArea.ColumnArea:
                return "C";
            case PivotArea.DataArea:
                return "D";
            case PivotArea.FilterArea:
                return "F";
            case PivotArea.RowArea:
                return "L";
            default:
                throw new Exception();
        }
    }

    private int ObtemCodigoCampoOLAP(DevExpress.Web.ASPxPivotGrid.PivotGridField field)
    {
        int codigoCampo = -1;
        string fieldName = field.Name;
        int indiceCodigoCampo = fieldName.LastIndexOf('#') + 1;
        if (indiceCodigoCampo > 0)
            codigoCampo = int.Parse(fieldName.Substring(indiceCodigoCampo));

        return codigoCampo;
    }

    public void RestaurarLayoutOLAP(ASPxPivotGrid pgDados)
    {
        StringBuilder comandoSql = new StringBuilder();
        comandoSql.AppendFormat(@"
        DECLARE @CodigoLista Int,
                @CodigoUsuario INT,
                @CodigoCampo INT

        SET @CodigoUsuario = {0}
        SET @CodigoLista = {1}
        ", getInfoSistema("IDUsuarioLogado"),
          codigoLista);

        comandoSql.AppendLine();
        foreach (DevExpress.Web.ASPxPivotGrid.PivotGridField field in pgDados.Fields.OfType<DevExpress.Web.ASPxPivotGrid.PivotGridField>().Where(f => f.CanShowInCustomizationForm))
        {
            int codigoCampo = ObtemCodigoCampoOLAP(field);

            if (codigoCampo > 0)
            {
                #region Comando SQL

                comandoSql.AppendFormat(@"   
    SET @CodigoCampo = {0}

     DELETE 
       FROM ListaCampoUsuario
      WHERE CodigoListaUsuario IN(SELECT lu.CodigoListaUsuario 
                                    FROM ListaUsuario AS lu 
                                    WHERE lu.CodigoLista = @CodigoLista 
                                      AND lu.CodigoUsuario = @CodigoUsuario)
        AND CodigoCampo = @CodigoCampo

 ",
                       codigoCampo);
                comandoSql.AppendLine();

                #endregion
            }
        }

        comandoSql.AppendFormat(@"   
     DELETE FROM ListaUsuario
       WHERE CodigoLista = @CodigoLista
         AND CodigoUsuario = @CodigoUsuario");
        comandoSql.AppendLine();

        int registrosAfetados = 0;
        execSQL(comandoSql.ToString(), ref registrosAfetados);
    }

    #endregion

    #endregion

    #region TREE LIST

    public void eventoClickMenuTreeList(ASPxMenu menu, string itemClick, DevExpress.Web.ASPxTreeList.ASPxTreeListExporter ASPxTreeListExporter1, string iniciaisLayoutTela, ASPxTreeList tlDados, string filtro)
    {
        setCodigoListaLayout(iniciaisLayoutTela);
        if (string.Equals(itemClick, Resources.traducao.salvar, StringComparison.CurrentCultureIgnoreCase))
        {
            SalvarCustomizacaoLayoutTreeList(tlDados, filtro);
        }
        else if (string.Equals(itemClick, Resources.traducao.restaurar, StringComparison.CurrentCultureIgnoreCase))
        {
            RestaurarLayoutTreeList(tlDados);
            InitDataTreeList(tlDados, iniciaisLayoutTela);
        }
        else
        {
            exportaTreeList(ASPxTreeListExporter1, itemClick);
        }
    }

    public void setaDefinicoesBotoesInserirExportarTreeList(ASPxMenu menu, bool podeIncluir, string funcaoJSbtnIncluir, bool mostraBtnIncluir, bool mostraBtnExportar, bool mostraBtnLayout, string iniciaisLayout, string tituloPagina, Page pagina, ASPxTreeList tlDados)
    {
        #region EXPORTAÇÃO

        try
        {
            if (getInfoSistema("IDUsuarioLogado") == null)
                pagina.Response.Redirect("~/erros/erroInatividade.aspx");
        }
        catch
        {
            pagina.Response.RedirectLocation = getPathSistema() + "erros/erroInatividade.aspx";
            pagina.Response.End();
        }

        DataSet dsTemp = getParametrosSistema("exportaOLAPTodosFormatos");

        bool exportaOLAPTodosFormatos = false;

        if ((DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0])) && dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() + "" != "")
        {
            exportaOLAPTodosFormatos = (dsTemp.Tables[0].Rows[0]["exportaOLAPTodosFormatos"].ToString() == "S");
        }

        DevExpress.Web.MenuItem btnExportar = menu.Items.FindByName("btnExportar");

        btnExportar.ClientVisible = mostraBtnExportar;

        if (!exportaOLAPTodosFormatos)
        {
            btnExportar.Items.Clear();
            btnExportar.Image.Url = "~/imagens/botoes/btnExcel.png";
            btnExportar.ToolTip = "Exportar para XLS";
        }

        #endregion

        #region INCLUIR

        DevExpress.Web.MenuItem btnIncluir = menu.Items.FindByName("btnIncluir");
        btnIncluir.ClientEnabled = podeIncluir;

        btnIncluir.ClientVisible = mostraBtnIncluir;

        if (podeIncluir == false)
            btnIncluir.Image.Url = "~/imagens/botoes/incluirRegDes.png";

        #endregion

        #region JS

        menu.ClientSideEvents.ItemClick =
        @"function(s, e){ 

            e.processOnServer = false;

            if(e.item.name == 'btnIncluir')
            {
                " + funcaoJSbtnIncluir + @"
            }		                     
	        else if(e.item.name != 'btnLayout')
	        {
                e.processOnServer = true;		                                        
	        }	
        }";

        #endregion

        #region LAYOUT

        DevExpress.Web.MenuItem btnLayout = menu.Items.FindByName("btnLayout");

        btnLayout.ClientVisible = mostraBtnLayout;

        if (mostraBtnLayout && !pagina.IsPostBack)
        {
            DataSet ds = getDataSet("SELECT 1 FROM Lista WHERE CodigoEntidade = " + getInfoSistema("CodigoEntidade") + " AND IniciaisListaControladaSistema = '" + iniciaisLayout + "'");

            if (ds.Tables[0].Rows.Count == 0)
            {
                int regAf = 0;

                execSQL(constroiInsertLayoutColunasTreeList(tlDados, iniciaisLayout, tituloPagina), ref regAf);
            }

            InitDataTreeList(tlDados, iniciaisLayout);
        }

        #endregion
    }

    public void configuraPainelBotoesTREELIST(HtmlTable tbBotoes)
    {
        string estiloFooter = "dxtlControl dxtlFooter";

        string cssPostfix = "", cssPath = "";

        

        if (Session["infoSistema"] != null)
        {
            getVisual(getInfoSistema("IDEstiloVisual").ToString(), ref cssPath, ref cssPostfix);
        }
        else
        {
            getVisual("Office2003Blue", ref cssPath, ref cssPostfix);
        }

        if (cssPostfix != "")
            estiloFooter = "dxtlControl_" + cssPostfix + " dxtlFooter_" + cssPostfix;

        tbBotoes.Attributes.Add("class", estiloFooter);

        tbBotoes.Style.Add("padding", "3px");

        tbBotoes.Style.Add("border-collapse", "collapse");

        tbBotoes.Style.Add("border-bottom", "none");
    }

    public void exportaTreeList(DevExpress.Web.ASPxTreeList.ASPxTreeListExporter tle, string tipoExportacao)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            string dataHora = DateTime.Now.ToString().Replace("/", "-").Replace(":", "_");

            string nomeArquivo = "";

            nomeArquivo = "Exportacao_" + dataHora;

            if (tipoExportacao == "PDF")
            {
                tle.WritePdfToResponse(nomeArquivo);
            }
            else if (tipoExportacao == "XLS" || tipoExportacao == "XLSX")
            {
                tle.WriteXlsToResponse(nomeArquivo);
            }
            else if (tipoExportacao == "RTF")
            {
                tle.WriteRtfToResponse(nomeArquivo);
            }
            else if (tipoExportacao == "HTML")
            {

            }
        }
    }

    #region GRAVAÇÃO e RECUPERAÇÃO DE LAYOUT

    public string constroiInsertLayoutColunasTreeList(ASPxTreeList tlDados, string iniciaisLista, string nomeLista)
    {
        StringBuilder comandoSql = new StringBuilder();

        comandoSql.AppendFormat(@"
BEGIN
    DECLARE @CodigoLista int

        INSERT INTO [dbo].[Lista]
	                (  [NomeLista]									, [GrupoMenu]									, [ItemMenu]
		                , [GrupoPermissao]						        , [ItemPermissao]							    , [IniciaisPermissao]
		                , [TituloLista]								, [ComandoSelect]							
		                , [IndicaPaginacao]						    , [QuantidadeItensPaginacao]	                , [IndicaOpcaoDisponivel]			
		                , [TipoLista]									, [URL]											, [CodigoEntidade]						
		                , [CodigoModuloMenu]					        , [IndicaListaZebrada]				            , [IniciaisListaControladaSistema])
	
	                SELECT  
			                '{0}'	  		                            , 'Geral'												, '{0}'
		                , 'Geral'									, 'Visualizar'											, 'CDIS000011'
		                , '{0}'				                        , ''
		                , 'S'									    , 50													, 'N'
		                , 'PROCESSO'								, NULL  												, {2}
		                , 'PRJ'										, 'S'													, '{1}'
		                                        
        SELECT @CodigoLista = scope_identity()", nomeLista, iniciaisLista, getInfoSistema("CodigoEntidade"));

        foreach (TreeListColumn coluna in tlDados.Columns.OfType<TreeListColumn>().Where(c => c.ShowInCustomizationForm))
        {
            if (coluna is TreeListEditDataColumn)
            {
                TreeListEditDataColumn col = coluna as TreeListEditDataColumn;
                string nomeCampo = col.FieldName;
                string tituloCampo = col.Caption;
                int ordemCampo = col.VisibleIndex;
                int ordemAgrupamentoCampo = -1;
                string tipoCampo = "VAR";
                string formato = col.PropertiesEdit.DisplayFormatString;
                string indicaAreaFiltro = "N";
                string tipoFiltro = "N";

                string indicaAgrupamento = "N";
                string tipoTotalizador = "Nenhum";

                string indicaAreaDado = "N";
                string indicaAreaColuna = "N";
                string indicaAreaLinha = "N";
                string areaDefault = "L";
                string indicaCampoVisivel = col.Visible ? "S" : "N";
                string indicaCampoControle = col.ShowInCustomizationForm ? "N" : "S";
                string iniciaisCampoControlado = "NULL";
                string indicaLink = "NULL";
                string alinhamentoCampo = "E";

                if (col.CellStyle.HorizontalAlign == HorizontalAlign.Center)
                    alinhamentoCampo = "C";
                else if (col.CellStyle.HorizontalAlign == HorizontalAlign.Right)
                    alinhamentoCampo = "D";

                string indicaCampoHierarquia = "N";
                string larguraColuna = col.Width.IsEmpty ? "NULL" : col.Width.Value.ToString();
                string tituloColunaAgrupadora = "NULL";
                #region Comando SQL

                comandoSql.AppendFormat(@"    

    INSERT INTO [dbo].[ListaCampo]([CodigoLista]
               ,[NomeCampo]
               ,[TituloCampo]
               ,[OrdemCampo]
               ,[OrdemAgrupamentoCampo]
               ,[TipoCampo]
               ,[Formato]
               ,[IndicaAreaFiltro]
               ,[TipoFiltro]
               ,[IndicaAgrupamento]
               ,[TipoTotalizador]
               ,[IndicaAreaDado]
               ,[IndicaAreaColuna]
               ,[IndicaAreaLinha]
               ,[AreaDefault]
               ,[IndicaCampoVisivel]
               ,[IndicaCampoControle]
               ,[IniciaisCampoControlado]
               ,[IndicaLink]
               ,[AlinhamentoCampo]
               ,[IndicaCampoHierarquia]
               ,[LarguraColuna]
               ,[TituloColunaAgrupadora]
               ,[IndicaColunaFixa]) VALUES (
                @CodigoLista
                , N'{0}'
                , N'{1}'
                , {2}
                , {3}
                , N'{4}'
                , '{5}'
                , N'{6}'
                , N'{7}'
                , N'{8}'
                , N'{9}'
                , N'{10}'
                , N'{11}'
                , N'{12}'
                , N'{13}'
                , N'{14}'
                , N'{15}'
                , {16}
                , {17}
                , N'{18}'
                , N'{19}'
                , {20}
                , {21}
                , N'{22}');
 ", nomeCampo
        , tituloCampo
        , ordemCampo
        , ordemAgrupamentoCampo
        , tipoCampo
        , formato.Replace("'", "\"")
        , indicaAreaFiltro
        , tipoFiltro
        , indicaAgrupamento
        , tipoTotalizador
        , indicaAreaDado
        , indicaAreaColuna
        , indicaAreaLinha
        , areaDefault
        , indicaCampoVisivel
        , indicaCampoControle
        , iniciaisCampoControlado
        , indicaLink
        , alinhamentoCampo
        , indicaCampoHierarquia
        , larguraColuna
        , tituloColunaAgrupadora
        , "N"
        , iniciaisLista);

                comandoSql.AppendLine();

                #endregion
            }
        }

        comandoSql.AppendLine("END");

        return comandoSql.ToString();
    }

    private void InitDataTreeList(ASPxTreeList tlDados, string iniciaisLayout)
    {
        dsLP = new DsListaProcessos();

        setCodigoListaLayout(iniciaisLayout);

        string comandoPreencheLista;
        string comandoPreencheListaCampo;
        string comandoPreencheListaFluxo;

        #region Comandos SQL

        comandoPreencheLista = string.Format(@"
 SELECT l.CodigoLista, 
		NomeLista, 
		GrupoMenu, 
		ItemMenu, 
		GrupoPermissao, 
		ItemPermissao, 
		IniciaisPermissao, 
		TituloLista, 
		ComandoSelect, 
		IndicaPaginacao, 
		ISNULL(lu.QuantidadeItensPaginacao, l.QuantidadeItensPaginacao) AS QuantidadeItensPaginacao, 
        lu.FiltroAplicado,
		IndicaOpcaoDisponivel, 
		TipoLista, 
		URL, 
		CodigoEntidade,
        CodigoModuloMenu,
        IndicaListaZebrada
   FROM Lista l left join ListaUsuario lu ON (lu.CodigoLista = l.CodigoLista AND lu.CodigoUsuario = {0}) 
  WHERE (l.CodigoLista = " + codigoLista + @")", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaCampo = string.Format(@"
 SELECT lc.CodigoCampo,
        lc.CodigoLista, 
        lc.NomeCampo, 
        lc.TituloCampo, 
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo) AS OrdemCampo, 
        ISNULL(lcu.OrdemAgrupamentoCampo, lc.OrdemAgrupamentoCampo) AS OrdemAgrupamentoCampo,
        lc.TipoCampo, 
        lc.Formato, 
        lc.IndicaAreaFiltro, 
        lc.TipoFiltro, 
        lc.IndicaAgrupamento, 
        lc.TipoTotalizador, 
        lc.IndicaAreaDado, 
        lc.IndicaAreaColuna, 
        lc.IndicaAreaLinha, 
        ISNULL(lcu.AreaDefault, lc.AreaDefault) AS AreaDefault, 
        ISNULL(lcu.IndicaCampoVisivel, lc.IndicaCampoVisivel) AS IndicaCampoVisivel, 
        lc.IndicaCampoControle,
        lc.IniciaisCampoControlado,
        lc.IndicaLink,
        (CASE WHEN lcu.CodigoCampo IS NOT NULL THEN 'S' ELSE 'N' END) AS IndicaCampoCustumizado,
        lc.AlinhamentoCampo,
        lc.IndicaCampoHierarquia,
        ISNULL(lcu.LarguraColuna, lc.LarguraColuna) AS LarguraColuna
   FROM ListaCampo AS lc LEFT JOIN
		ListaUsuario lu ON lu.CodigoLista = lc.CodigoLista AND 
						   lu.CodigoUsuario = {0} LEFT JOIN
        ListaCampoUsuario lcu ON lc.CodigoCampo = lcu.CodigoCampo AND 
                                 lcu.CodigoListaUsuario = lu.CodigoListaUsuario
  WHERE (lc.CodigoLista = " + codigoLista + @")
  ORDER BY
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo)", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaFluxo = @"
 SELECT CodigoLista, 
		CodigoFluxo, 
		TituloMenu
   FROM ListaFluxo
  WHERE (CodigoLista = " + codigoLista + ")";

        #endregion

        FillData(dsLP.Lista, comandoPreencheLista);
        FillData(dsLP.ListaCampo, comandoPreencheListaCampo);
        FillData(dsLP.ListaFluxo, comandoPreencheListaFluxo);

        DsListaProcessos.ListaRow item = dsLP.Lista.Single();

        SetTreeListSettings(item, tlDados);
    }

    private void SetTreeListSettings(DsListaProcessos.ListaRow lista, ASPxTreeList tlDados)
    {
        foreach (var campo in lista.GetListaCampoRows())
            AddTreeListColumn(campo, tlDados);

        bool possuiPaginacao = VerificaVerdadeiroOuFalso(lista.IndicaPaginacao);

        if (possuiPaginacao)
            tlDados.SettingsPager.Mode = TreeListPagerMode.ShowPager;
        else
            tlDados.SettingsPager.Mode = TreeListPagerMode.ShowAllNodes;

        if (possuiPaginacao)
            tlDados.SettingsPager.PageSize = lista.QuantidadeItensPaginacao;
    }

    private static string GetKeyFieldName(DsListaProcessos.ListaRow lista)
    {
        DsListaProcessos.ListaCampoRow[] campos = lista.GetListaCampoRows();
        DsListaProcessos.ListaCampoRow campoChavePrimaria =
            campos.SingleOrDefault(c => c.IndicaCampoHierarquia.Equals("P"));

        if (campoChavePrimaria == null) return string.Empty;

        return campoChavePrimaria.NomeCampo;
    }

    private static string GetParentFieldName(DsListaProcessos.ListaRow lista)
    {
        DsListaProcessos.ListaCampoRow[] campos = lista.GetListaCampoRows();
        DsListaProcessos.ListaCampoRow campoSuperiorEstruturaHierarquica =
            campos.SingleOrDefault(c => c.IndicaCampoHierarquia.Equals("S"));

        if (campoSuperiorEstruturaHierarquica == null) return string.Empty;

        return campoSuperiorEstruturaHierarquica.NomeCampo;
    }

    private void AddTreeListColumn(DsListaProcessos.ListaCampoRow campo, ASPxTreeList tlDados)
    {

        if (tlDados.Columns[campo.NomeCampo] is TreeListImageColumn)
        {
            #region Bullet

            TreeListImageColumn colImg = (TreeListImageColumn)tlDados.Columns[campo.NomeCampo];
            colImg.Caption = campo.TituloCampo;
            colImg.FieldName = campo.NomeCampo;
            colImg.Name = string.Format("fieldBLT_{0}#{1}", campo.NomeCampo, campo.CodigoCampo);
            colImg.PropertiesImage.ImageUrlFormatString = "~/imagens/{0}.gif";
            colImg.Visible = VerificaVerdadeiroOuFalso(campo.IndicaCampoVisivel);
            colImg.VisibleIndex = campo.OrdemCampo;
            if (!campo.IsLarguraColunaNull())
                colImg.Width = new Unit(campo.LarguraColuna, UnitType.Pixel);

            #region Tipo filtro

            //Não suporta filtros nativamente

            #endregion

            #region Alinhamento

            HorizontalAlign alinhamento = HorizontalAlign.Center;
            switch (campo.AlinhamentoCampo.ToUpper())
            {
                case "E":
                    alinhamento = HorizontalAlign.Left;
                    break;
                case "D":
                    alinhamento = HorizontalAlign.Right;
                    break;
                case "C":
                    alinhamento = HorizontalAlign.Center;
                    break;
            }
            colImg.CellStyle.HorizontalAlign = alinhamento;
            colImg.HeaderStyle.HorizontalAlign = alinhamento;

            #endregion

            tlDados.Columns.Add(colImg);

            #endregion
        }
        else
        {
            #region Outros

            TreeListTextColumn colTxt = (TreeListTextColumn)tlDados.Columns[campo.NomeCampo];

            if (VerificaVerdadeiroOuFalso(campo.IndicaCampoControle))
            {
                colTxt.Visible = false;
                colTxt.ShowInCustomizationForm = false;
                string iniciais = campo.IniciaisCampoControlado;
                if (!string.IsNullOrEmpty(iniciais))
                    colTxt.Name = string.Format("colCC_{0}", iniciais);
            }
            else
            {
                colTxt.Caption = campo.TituloCampo;
                colTxt.Name = string.Format("col#{0}", campo.CodigoCampo);
                colTxt.PropertiesTextEdit.DisplayFormatString = campo.Formato;
                colTxt.Visible = VerificaVerdadeiroOuFalso(campo.IndicaCampoVisivel);
                if (VerificaVerdadeiroOuFalso(campo.IndicaCampoVisivel))
                    colTxt.VisibleIndex = campo.OrdemCampo;
                if (!campo.IsLarguraColunaNull())
                    colTxt.Width = new Unit(campo.LarguraColuna, UnitType.Pixel);

                #region Tipo campo

                switch (campo.TipoCampo.ToUpper())
                {
                    default:
                        break;
                }

                #endregion

                #region Tipo filtro

                #endregion

                #region Tipo totalizados

                string tipoTotalizador = campo.TipoTotalizador.ToUpper();
                SummaryItemType summaryType = GetSummaryType(tipoTotalizador);
                TreeListSummaryItem summaryItem = new TreeListSummaryItem();
                summaryItem.DisplayFormat = campo.Formato;
                summaryItem.FieldName = campo.NomeCampo;
                summaryItem.ShowInColumn = campo.NomeCampo;
                summaryItem.SummaryType = summaryType;
                tlDados.Summary.Add(summaryItem);

                #endregion

                #region Alinhamento

                HorizontalAlign alinhamento = HorizontalAlign.Center;
                switch (campo.AlinhamentoCampo.ToUpper())
                {
                    case "E":
                        alinhamento = HorizontalAlign.Left;
                        break;
                    case "D":
                        alinhamento = HorizontalAlign.Right;
                        break;
                    case "C":
                        alinhamento = HorizontalAlign.Center;
                        break;
                }
                colTxt.CellStyle.HorizontalAlign = alinhamento;
                colTxt.HeaderStyle.HorizontalAlign = alinhamento;

                #endregion
            }

            #endregion
        }
    }

    private void SalvarCustomizacaoLayoutTreeList(ASPxTreeList tlDados, string filtroAplicado)
    {
        StringBuilder comandoSql = new StringBuilder();
        int registrosAfetados = 0;

        #region Atualizando a tabela ListaUsuario

        int qtdeItensPagina;
        qtdeItensPagina = 100;

        comandoSql.Append(@"
DECLARE @CodigoLista Int,
        @CodigoUsuario Int,
        @QuantidadeItensPaginacao SmallInt,
        @FiltroAplicado VARCHAR(MAX),
        @CodigoListaUsuario BIGINT");

        comandoSql.AppendLine();

        comandoSql.AppendFormat(@"
	SET @CodigoLista = {0}
	SET @CodigoUsuario = {1}
	SET @QuantidadeItensPaginacao = {2}
	SET @FiltroAplicado = '{3}'
            
 SELECT TOP 1 @CodigoListaUsuario = CodigoListaUsuario 
   FROM ListaUsuario AS lu 
  WHERE lu.CodigoLista = @CodigoLista 
    AND lu.CodigoUsuario = @CodigoUsuario

IF @CodigoListaUsuario IS NOT NULL
BEGIN
     UPDATE ListaUsuario
        SET QuantidadeItensPaginacao = @QuantidadeItensPaginacao,
			FiltroAplicado = @FiltroAplicado
      WHERE CodigoListaUsuario = @CodigoListaUsuario
END
ELSE
BEGIN
     INSERT INTO ListaUsuario(CodigoUsuario, CodigoLista, QuantidadeItensPaginacao, FiltroAplicado, IndicaListaPadrao) VALUES (@CodigoUsuario, @CodigoLista, @QuantidadeItensPaginacao, @FiltroAplicado, 'S')
   
        SET @CodigoListaUsuario = SCOPE_IDENTITY()
END"
            , codigoLista, getInfoSistema("IDUsuarioLogado"), qtdeItensPagina, filtroAplicado.Replace("'", "''"));

        comandoSql.AppendLine();

        #endregion

        comandoSql.Append(@"
DECLARE @CodigoCampo INT,
        @OrdemCampo SMALLINT,
        @OrdemAgrupamentoCampo SMALLINT,
        @AreaDefault CHAR(1),
        @IndicaCampoVisivel CHAR(1),
        @LarguraColuna SMALLINT");
        comandoSql.AppendLine();
        foreach (TreeListEditDataColumn col in tlDados.Columns.OfType<TreeListEditDataColumn>().Where(c => c.ShowInCustomizationForm))
        {
            int ordemCampo = col.Visible ? col.VisibleIndex : -1;
            int ordemAgrupamentoCampo = 1;
            string areaDefault = "L";
            string indicaCampoVisivel = col.Visible ? "S" : "N";
            int codigoCampo = ObtemCodigoCampoTreeList(col);
            double larguraColuna = col.Width.IsEmpty ? 100 : col.Width.Value;

            #region Comando SQL

            comandoSql.AppendFormat(@"
    SET @CodigoUsuario = {0}
    SET @CodigoCampo = {1}
    SET @OrdemCampo = {2}
    SET @OrdemAgrupamentoCampo = {3}
    SET @AreaDefault = '{4}'
    SET @IndicaCampoVisivel = '{5}'
    SET @LarguraColuna = {6}

 IF EXISTS(SELECT NULL FROM ListaCampoUsuario AS lcu WHERE lcu.CodigoCampo = @CodigoCampo AND lcu.CodigoListaUsuario = @CodigoListaUsuario)
 BEGIN
    UPDATE ListaCampoUsuario
       SET OrdemCampo = @OrdemCampo,
           OrdemAgrupamentoCampo = @OrdemAgrupamentoCampo,
           AreaDefault = @AreaDefault,
           IndicaCampoVisivel = @IndicaCampoVisivel,
           LarguraColuna = @LarguraColuna
      WHERE CodigoCampo = @CodigoCampo
        AND CodigoListaUsuario = @CodigoListaUsuario
 END
 ELSE
 BEGIN
    INSERT INTO ListaCampoUsuario(
           CodigoCampo,
           CodigoListaUsuario,
           OrdemCampo,
           OrdemAgrupamentoCampo,
           AreaDefault,
           IndicaCampoVisivel,
           LarguraColuna)
     VALUES(
           @CodigoCampo,
           @CodigoListaUsuario,
           @OrdemCampo,
           @OrdemAgrupamentoCampo,
           @AreaDefault,
           @IndicaCampoVisivel,
           @LarguraColuna)
 END",
     getInfoSistema("IDUsuarioLogado"),
     codigoCampo,
     ordemCampo,
     ordemAgrupamentoCampo,
     areaDefault,
     indicaCampoVisivel,
     larguraColuna);
            comandoSql.AppendLine();

            #endregion
        }

        execSQL(comandoSql.ToString(), ref registrosAfetados);
    }

    private int ObtemCodigoCampoTreeList(TreeListEditDataColumn col)
    {
        int codigoCampo = -1;
        string columnName = col.Name;
        int indiceCodigoCampo = columnName.LastIndexOf('#') + 1;
        if (indiceCodigoCampo > 0)
            codigoCampo = int.Parse(columnName.Substring(indiceCodigoCampo));

        return codigoCampo;
    }

    private void RestaurarLayoutTreeList(ASPxTreeList tlDados)
    {
        StringBuilder comandoSql = new StringBuilder(@"
DECLARE @CodigoUsuario INT");
        comandoSql.AppendLine();


        #region Comando SQL

        comandoSql.AppendFormat(@"
    SET @CodigoUsuario = {0}

  DELETE 
       FROM ListaCampoUsuario
      WHERE CodigoListaUsuario IN(SELECT lu.CodigoListaUsuario 
                                    FROM ListaUsuario AS lu 
                                    WHERE lu.CodigoLista = {1}
                                      AND lu.CodigoUsuario = @CodigoUsuario)",
               getInfoSistema("IDUsuarioLogado"),
               codigoLista);
        comandoSql.AppendLine();

        #endregion

        comandoSql.AppendFormat(@"   
     DELETE FROM ListaUsuario
       WHERE CodigoLista = {0}
         AND CodigoUsuario = @CodigoUsuario", codigoLista);
        comandoSql.AppendLine();

        int registrosAfetados = 0;
        execSQL(comandoSql.ToString(), ref registrosAfetados);

    }

    #endregion

    #endregion

    #region GRAVAÇÃO e RECUPERAÇÃO DE LAYOUT

    public void setCodigoListaLayout(string iniciaisLista)
    {
        string comandoSQL;
        int codLista = -1;

        comandoSQL = string.Format("SELECT l.[CodigoLista] FROM {0}.{1}.[Lista] AS [l] WHERE l.[CodigoEntidade] = {2} AND l.[IniciaisListaControladaSistema] = '{3}' ", getDbName(), getDbOwner(), getInfoSistema("CodigoEntidade"), iniciaisLista);

        DataSet ds = getDataSet(comandoSQL);
        if ((DataSetOk(ds)) && (DataTableOk(ds.Tables[0])))
            codLista = int.Parse(ds.Tables[0].Rows[0]["CodigoLista"].ToString());

        codigoLista = codLista;
    }

    public void InitData(ASPxGridView gvDados, string iniciaisLista)
    {
        dsLP = new DsListaProcessos();

        setCodigoListaLayout(iniciaisLista);

        string comandoPreencheLista;
        string comandoPreencheListaCampo;
        string comandoPreencheListaFluxo;

        #region Comandos SQL

        comandoPreencheLista = string.Format(@"
 SELECT l.CodigoLista, 
		NomeLista, 
		GrupoMenu, 
		ItemMenu, 
		GrupoPermissao, 
		ItemPermissao, 
		IniciaisPermissao, 
		TituloLista, 
		ComandoSelect, 
		IndicaPaginacao, 
		ISNULL(lu.QuantidadeItensPaginacao, l.QuantidadeItensPaginacao) AS QuantidadeItensPaginacao, 
        lu.FiltroAplicado,
		IndicaOpcaoDisponivel, 
		TipoLista, 
		URL, 
		CodigoEntidade,
        CodigoModuloMenu,
        IndicaListaZebrada
   FROM Lista l left join ListaUsuario lu ON (lu.CodigoLista = l.CodigoLista AND lu.CodigoUsuario = {0}) 
  WHERE (l.CodigoLista = " + codigoLista + @")", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaCampo = string.Format(@"
 SELECT lc.CodigoCampo,
        lc.CodigoLista, 
        lc.NomeCampo, 
        lc.TituloCampo, 
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo) AS OrdemCampo, 
        ISNULL(lcu.OrdemAgrupamentoCampo, lc.OrdemAgrupamentoCampo) AS OrdemAgrupamentoCampo,
        lc.TipoCampo, 
        lc.Formato, 
        lc.IndicaAreaFiltro, 
        lc.TipoFiltro, 
        lc.IndicaAgrupamento, 
        lc.TipoTotalizador, 
        lc.IndicaAreaDado, 
        lc.IndicaAreaColuna, 
        lc.IndicaAreaLinha, 
        ISNULL(lcu.AreaDefault, lc.AreaDefault) AS AreaDefault, 
        ISNULL(lcu.IndicaCampoVisivel, lc.IndicaCampoVisivel) AS IndicaCampoVisivel, 
        lc.IndicaCampoControle,
        lc.IniciaisCampoControlado,
        lc.IndicaLink,
        (CASE WHEN lcu.CodigoCampo IS NOT NULL THEN 'S' ELSE 'N' END) AS IndicaCampoCustumizado,
        lc.AlinhamentoCampo,
        lc.IndicaCampoHierarquia,
        ISNULL(lcu.LarguraColuna, lc.LarguraColuna) AS LarguraColuna
   FROM ListaCampo AS lc LEFT JOIN
		ListaUsuario lu ON lu.CodigoLista = lc.CodigoLista AND 
						   lu.CodigoUsuario = {0} LEFT JOIN
        ListaCampoUsuario lcu ON lc.CodigoCampo = lcu.CodigoCampo AND 
                                 lcu.CodigoListaUsuario = lu.CodigoListaUsuario
  WHERE (lc.CodigoLista = " + codigoLista + @")
  ORDER BY
        ISNULL(lcu.OrdemCampo, lc.OrdemCampo)", getInfoSistema("IDUsuarioLogado"));

        comandoPreencheListaFluxo = @"
 SELECT CodigoLista, 
		CodigoFluxo, 
		TituloMenu
   FROM ListaFluxo
  WHERE (CodigoLista = " + codigoLista + ")";

        #endregion

        FillData(dsLP.Lista, comandoPreencheLista);
        FillData(dsLP.ListaCampo, comandoPreencheListaCampo);
        FillData(dsLP.ListaFluxo, comandoPreencheListaFluxo);

        DsListaProcessos.ListaRow item = dsLP.Lista.Single();

        SetGridSettings(item, gvDados);
    }

    private void FillData(DataTable dt, string comandoSql)
    {
        SqlDataAdapter da = new SqlDataAdapter(comandoSql, ConnectionString);
        SqlParameter p = da.SelectCommand.Parameters.Add("@CodigoLista", SqlDbType.Int);
        p.Value = codigoLista;
        da.Fill(dt);
    }

    private void SetGridSettings(DsListaProcessos.ListaRow item, ASPxGridView gvDados)
    {
        foreach (var campo in item.GetListaCampoRows())
            AddGridColumn(campo, gvDados);

        bool possuiPaginacao = VerificaVerdadeiroOuFalso(item.IndicaPaginacao);

        if (possuiPaginacao)
            gvDados.SettingsPager.Mode = GridViewPagerMode.ShowPager;
        else
            gvDados.SettingsPager.Mode = GridViewPagerMode.ShowAllRecords;

        if (possuiPaginacao)
            gvDados.SettingsPager.PageSize = item.QuantidadeItensPaginacao;

        gvDados.FilterExpression = item.FiltroAplicado;
    }

    private void AddGridColumn(DsListaProcessos.ListaCampoRow campo, ASPxGridView gvDados)
    {
        
        GridViewDataColumn colTxt = (GridViewDataColumn)gvDados.Columns[campo.NomeCampo];

        if (colTxt != null)
        {
            if (VerificaVerdadeiroOuFalso(campo.IndicaCampoControle))
            {
                colTxt.Visible = false;
                colTxt.ShowInCustomizationForm = false;
                string iniciais = campo.IniciaisCampoControlado;
                if (!string.IsNullOrEmpty(iniciais))
                    colTxt.Name = string.Format("colCC_{0}", iniciais);
            }
            else
            {
                colTxt.GroupIndex = campo.OrdemAgrupamentoCampo;
                colTxt.Name = string.Format("col#{0}", campo.CodigoCampo);
                colTxt.Caption = campo.TituloCampo;
                colTxt.VisibleIndex = campo.OrdemCampo;
                colTxt.Visible = VerificaVerdadeiroOuFalso(campo.IndicaCampoVisivel);
                colTxt.Settings.AllowGroup = campo.IndicaAgrupamento == "S" ? DefaultBoolean.True : DefaultBoolean.False;

                if (!colTxt.PropertiesEdit.DisplayFormatString.Contains("<img"))
                    colTxt.PropertiesEdit.DisplayFormatString = campo.Formato;

                if (!campo.IsLarguraColunaNull())
                    colTxt.Width = new Unit(campo.LarguraColuna, UnitType.Pixel);

                #region Tipo campo

                switch (campo.TipoCampo.ToUpper())
                {
                    default:
                        break;
                }

                #endregion

                #region Tipo filtro

                colTxt.Settings.ShowFilterRowMenu = DefaultBoolean.True;
                colTxt.SettingsHeaderFilter.Mode = GridHeaderFilterMode.CheckedList;
                colTxt.Settings.AllowAutoFilter = campo.TipoFiltro.ToUpper() == "E" || campo.TipoFiltro.ToUpper() == "L" ?
                    DefaultBoolean.True : DefaultBoolean.False;
                colTxt.Settings.AllowHeaderFilter = campo.TipoFiltro.ToUpper() == "C" || campo.TipoFiltro.ToUpper() == "L" ?
                    DefaultBoolean.True : DefaultBoolean.False;
                colTxt.Settings.AutoFilterCondition = AutoFilterCondition.Contains;

                #endregion

                #region Tipo totalizados

                string nomeCampo = campo.NomeCampo;
                string tipoTotalizador = campo.TipoTotalizador.ToUpper();
                SummaryItemType summaryType = GetSummaryType(tipoTotalizador);
                ASPxSummaryItem summaryItem = new ASPxSummaryItem(nomeCampo, summaryType);
                summaryItem.DisplayFormat = campo.Formato;
                gvDados.GroupSummary.Add(summaryItem);
                gvDados.TotalSummary.Add(summaryItem);

                #endregion

                #region Alinhamento

                HorizontalAlign alinhamento = HorizontalAlign.Center;
                switch (campo.AlinhamentoCampo.ToUpper())
                {
                    case "E":
                        alinhamento = HorizontalAlign.Left;
                        break;
                    case "D":
                        alinhamento = HorizontalAlign.Right;
                        break;
                    case "C":
                        alinhamento = HorizontalAlign.Center;
                        break;
                }
                colTxt.CellStyle.HorizontalAlign = alinhamento;
                colTxt.HeaderStyle.HorizontalAlign = alinhamento;

                #endregion
            }
        }
         
    }

    private static SummaryItemType GetSummaryType(string tipoTotalizador)
    {
        SummaryItemType summaryType;
        switch (tipoTotalizador)
        {
            case "CONTAR":
                summaryType = SummaryItemType.Count;
                break;
            case "MÉDIA":
                summaryType = SummaryItemType.Average;
                break;
            case "SOMA":
                summaryType = SummaryItemType.Sum;
                break;
            default:
                summaryType = SummaryItemType.None;
                break;
        }
        return summaryType;
    }

    private bool VerificaVerdadeiroOuFalso(string valor)
    {
        if (valor == null)
            return false;
        return valor.ToLower().Equals("s");
    }

    public void SalvarCustomizacaoLayout(ASPxGridView gvDados)
    {
        StringBuilder comandoSql = new StringBuilder();
        int registrosAfetados = 0;

        #region Atualizando a tabela ListaUsuario

        int qtdeItensPagina;
        string filtroAplicado;
        qtdeItensPagina = gvDados.SettingsPager.PageSize;
        filtroAplicado = gvDados.FilterExpression;

        comandoSql.Append(@"
DECLARE @CodigoLista Int,
        @CodigoUsuario Int,
        @QuantidadeItensPaginacao SmallInt,
        @FiltroAplicado VARCHAR(MAX),
        @CodigoListaUsuario BIGINT");

        comandoSql.AppendLine();

        comandoSql.AppendFormat(@"
	SET @CodigoLista = {0}
	SET @CodigoUsuario = {1}
	SET @QuantidadeItensPaginacao = {2}
	SET @FiltroAplicado = '{3}'
            
 SELECT TOP 1 @CodigoListaUsuario = CodigoListaUsuario 
   FROM ListaUsuario AS lu 
  WHERE lu.CodigoLista = @CodigoLista 
    AND lu.CodigoUsuario = @CodigoUsuario

IF @CodigoListaUsuario IS NOT NULL
BEGIN
     UPDATE ListaUsuario
        SET QuantidadeItensPaginacao = @QuantidadeItensPaginacao,
			FiltroAplicado = @FiltroAplicado
      WHERE CodigoListaUsuario = @CodigoListaUsuario
END
ELSE
BEGIN
     INSERT INTO ListaUsuario(CodigoUsuario, CodigoLista, QuantidadeItensPaginacao, FiltroAplicado, IndicaListaPadrao) VALUES (@CodigoUsuario, @CodigoLista, @QuantidadeItensPaginacao, @FiltroAplicado, 'S')
   
        SET @CodigoListaUsuario = SCOPE_IDENTITY()
END"
            , codigoLista, getInfoSistema("IDUsuarioLogado"), qtdeItensPagina, filtroAplicado.Replace("'", "''"));

        comandoSql.AppendLine();

        #endregion

        comandoSql.Append(@"
DECLARE @CodigoCampo INT,
        @OrdemCampo SMALLINT,
        @OrdemAgrupamentoCampo SMALLINT,
        @AreaDefault CHAR(1),
        @IndicaCampoVisivel CHAR(1),
        @LarguraColuna SMALLINT");
        comandoSql.AppendLine();
        foreach (GridViewColumn coluna in gvDados.AllColumns.Where(c => c.ShowInCustomizationForm))
        {
            if (coluna is GridViewEditDataColumn)
            {
                GridViewEditDataColumn col = coluna as GridViewEditDataColumn;

                int ordemCampo = col.VisibleIndex;
                int ordemAgrupamentoCampo = col.GroupIndex;
                string areaDefault = "L";
                string indicaCampoVisivel = col.Visible ? "S" : "N";
                int codigoCampo = ObtemCodigoCampo(col);
                double larguraColuna = col.Width.IsEmpty ? 100 : col.Width.Value;

                #region Comando SQL

                comandoSql.AppendFormat(@"
    SET @CodigoUsuario = {0}
    SET @CodigoCampo = {1}
    SET @OrdemCampo = {2}
    SET @OrdemAgrupamentoCampo = {3}
    SET @AreaDefault = '{4}'
    SET @IndicaCampoVisivel = '{5}'
    SET @LarguraColuna = {6}

IF EXISTS(SELECT 1 FROM ListaCampoUsuario AS lcu WHERE lcu.CodigoCampo = @CodigoCampo AND lcu.CodigoListaUsuario = @CodigoListaUsuario)
BEGIN
     UPDATE ListaCampoUsuario
        SET OrdemCampo = @OrdemCampo,
            OrdemAgrupamentoCampo = @OrdemAgrupamentoCampo,
            AreaDefault = @AreaDefault,
            IndicaCampoVisivel = @IndicaCampoVisivel,
            LarguraColuna = @LarguraColuna
      WHERE CodigoCampo = @CodigoCampo
        AND CodigoListaUsuario = @CodigoListaUsuario
END
ELSE
BEGIN
     INSERT INTO ListaCampoUsuario(
            CodigoCampo,
            CodigoListaUsuario,
            OrdemCampo,
            OrdemAgrupamentoCampo,
            AreaDefault,
            IndicaCampoVisivel,
            LarguraColuna)
     VALUES(
            @CodigoCampo,
            @CodigoListaUsuario,
            @OrdemCampo,
            @OrdemAgrupamentoCampo,
            @AreaDefault,
            @IndicaCampoVisivel,
            @LarguraColuna)
END",
         getInfoSistema("IDUsuarioLogado"),
         codigoCampo,
         ordemCampo,
         ordemAgrupamentoCampo,
         areaDefault,
         indicaCampoVisivel,
         larguraColuna);
                comandoSql.AppendLine();

                #endregion
            }
        }

        execSQL(comandoSql.ToString(), ref registrosAfetados);
    }

    private int ObtemCodigoCampo(GridViewColumn col)
    {
        int codigoCampo = -1;
        string columnName = col.Name;
        int indiceCodigoCampo = columnName.LastIndexOf('#') + 1;
        if (indiceCodigoCampo > 0)
            codigoCampo = int.Parse(columnName.Substring(indiceCodigoCampo));

        return codigoCampo;
    }

    public void RestaurarLayout(ASPxGridView gvDados)
    {
        StringBuilder comandoSql = new StringBuilder(@"
DECLARE @CodigoUsuario INT,
        @CodigoCampo INT");
        comandoSql.AppendLine();
        foreach (GridViewColumn coluna in gvDados.AllColumns.Where(c => c.ShowInCustomizationForm))
        {
            if (coluna is GridViewEditDataColumn)
            {
                GridViewEditDataColumn col = coluna as GridViewEditDataColumn;
                int codigoCampo = ObtemCodigoCampo(col);

                #region Comando SQL

                comandoSql.AppendFormat(@"
    SET @CodigoUsuario = {0}
    SET @CodigoCampo = {1}

     DELETE 
       FROM ListaCampoUsuario
      WHERE CodigoListaUsuario IN(SELECT lu.CodigoListaUsuario 
                                    FROM ListaUsuario AS lu 
                                    WHERE lu.CodigoLista = {2} 
                                      AND lu.CodigoUsuario = @CodigoUsuario)
        AND CodigoCampo = @CodigoCampo",
                       getInfoSistema("IDUsuarioLogado"),
                       codigoCampo,
                       codigoLista);
                comandoSql.AppendLine();

                #endregion
            }
        }

        comandoSql.AppendFormat(@"   
     DELETE FROM ListaUsuario
       WHERE CodigoLista = {0}
         AND CodigoUsuario = @CodigoUsuario", codigoLista);
        comandoSql.AppendLine();

        int registrosAfetados = 0;
        execSQL(comandoSql.ToString(), ref registrosAfetados);

    }

    #endregion

    #endregion

    #region Planilha Orçamento Aprovado CA

    public DataSet getPlanilhaOrcamentoAprovadoCA()
    {
        string comandoSQL = string.Format(
            @"SELECT * FROM {0}.{1}.f_uhe_PlanilhaOrcamento()
               ", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    #endregion

}
}