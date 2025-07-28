using DevExpress.Web;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region Orçamento
    public DataSet getPlanoContasFluxoCaixa(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
           @" SELECT pc.CodigoConta
                   ,pc.DescricaoConta
                   ,pcs2.DescricaoConta AS Diretoria
                   ,pcs.DescricaoConta AS Departamento
                   ,pc.TipoConta
                   ,pc.EntradaSaida
                   ,pc.CodigoContaSuperior
                   ,pc.CodigoEntidade
                   ,pc.IndicaContaAnalitica
                   ,pc.CodigoReservadoGrupoConta
                   ,pcs.EntradaSaida as EntradaSaidaContaSuperior
              FROM {0}.{1}.PlanoContasFluxoCaixa pc left JOIN
		           {0}.{1}.PlanoContasFluxoCaixa pcs ON pcs.CodigoConta = pc.CodigoContaSuperior left JOIN 
		           {0}.{1}.PlanoContasFluxoCaixa pcs2 ON pcs2.CodigoConta = pcS.CodigoContaSuperior		           
             WHERE pc.CodigoEntidade = {2} {3}  
          ORDER BY pc.CodigoConta", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);

    }

    public DataSet getPlanoContaSuperior(int codigoEntidade, string whereTipoConta)
    {
        string comandoSQL = string.Format(
            @"SELECT pc.CodigoConta
                    ,pc.DescricaoConta 
                    ,pc.CodigoContaSuperior
               FROM {0}.{1}.PlanoContasFluxoCaixa pc 
              WHERE pc.CodigoEntidade = {2}  {3}", bancodb, Ownerdb, codigoEntidade, whereTipoConta);
        return getDataSet(comandoSQL);
    }

    public bool incluiPlanoDeContasFluxoCaixa(string descricaoConta, string codigoContaSuperior, int codigoEntidade, string codigoReservadoGrupoConta, string tipoConta, string EntradaSaida, string indicaContaAnalitica, ref string mensagemErro)
    {
        bool retorno = false;
        string trataCodigoContaSuperior = (codigoContaSuperior == "") ? "null" : codigoContaSuperior;
        string trataCodigoReservadoGrupoConta = (codigoReservadoGrupoConta == "") ? "null" : "'" + codigoReservadoGrupoConta + "'";
        string comandoSQL = string.Format(
       @"BEGIN
            DECLARE @CodContaSuperior as int         
            SET @CodContaSuperior = {3}          
            INSERT INTO 
            {0}.{1}.PlanoContasFluxoCaixa(DescricaoConta,EntradaSaida
                                       ,CodigoContaSuperior,TipoConta
                                       ,CodigoEntidade,IndicaContaAnalitica
                                       ,CodigoReservadoGrupoConta)
                                VALUES('{2}','{7}'
                                       ,{3},'{4}'
                                       ,{5},'{8}'
                                           ,{6})
          END"
             , bancodb, Ownerdb
             , descricaoConta
             , trataCodigoContaSuperior, tipoConta
             , codigoEntidade, trataCodigoReservadoGrupoConta, EntradaSaida, indicaContaAnalitica);
        try
        {
            int registrosAfetados = 0;
            retorno = execSQL(comandoSQL, ref registrosAfetados);
            return retorno;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool verificaExclusaoPlanoDeContasFluxoCaixa(int codigoConta, ref string mensagemErro)
    {
        bool retorno = false;
        comandoSQL = string.Format(
       @"SELECT 1 
           FROM {0}.{1}.PlanoContasFluxoCaixa
          WHERE CodigoContaSuperior = {2}", bancodb, Ownerdb, codigoConta);
        try
        {

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                retorno = false;
            }
            else
            {
                retorno = true;
            }
            return retorno;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool excluiPlanoDeContasFluxoCaixa(int codigoConta, ref string mensagemErro)
    {
        bool retorno = false;
        comandoSQL = string.Format(
       @"BEGIN
              IF NOT EXISTS(SELECT 1 
                              FROM {0}.{1}.[PlanoContasFluxoCaixa]
                             WHERE CodigoContaSuperior = {2})
               BEGIN
               DELETE FROM {0}.{1}.[PlanoContasFluxoCaixa]
                     WHERE CodigoConta = {2}
               END
        END", bancodb, Ownerdb, codigoConta);
        try
        {
            int registrosAfetados = 0;
            retorno = execSQL(comandoSQL, ref registrosAfetados);
            return retorno;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool atualizaPlanoDeContasFluxoCaixa(string descricaoConta, string codigoContaSuperior, int codigoContaAtual, string codigoReservadoGrupoConta, string EntradaSaida, string tipoConta, string IndicaContaAnalitica, ref string mensagemErro)
    {
        bool retorno = false;
        string trataCodigoContaSuperior = (codigoContaSuperior == "") ? "null" : codigoContaSuperior;

        //se o codigo da conta superior desejado for a propria conta, não aceitar
        string permiteAtualizarCodSuperior = ",[CodigoContaSuperior] = " + trataCodigoContaSuperior;
        //if (trataCodigoContaSuperior != "null" && codigoContaAtual == int.Parse(trataCodigoContaSuperior))
        //{
        permiteAtualizarCodSuperior = "";
        //}

        comandoSQL = string.Format(
            @"UPDATE {0}.{1}.PlanoContasFluxoCaixa
                 SET DescricaoConta = '{2}'
                    ,EntradaSaida = '{6}'
                     {3}
                    ,CodigoReservadoGrupoConta = '{4}'
                    ,TipoConta = '{7}'
                    ,IndicaContaAnalitica = '{8}' 
               WHERE CodigoConta = {5}", bancodb, Ownerdb, descricaoConta, permiteAtualizarCodSuperior, codigoReservadoGrupoConta, codigoContaAtual, EntradaSaida, tipoConta, IndicaContaAnalitica);

        try
        {
            int registrosAfetados = 0;
            retorno = execSQL(comandoSQL, ref registrosAfetados);
            return retorno;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public DataSet getDesempenhoOrcamentoProjetos(int codigoUsuario, int mes, int orcamento, int codigoEntidade, string where)
    {
        // Busca o nome do banco de dados do sistema de orçamento
        string nomeBancoSistemaOrcamento = "dbCdisOrcamento";

        DataSet dsParametros = getParametrosSistema("nomeBancoSistemaOrcamento");

        if (DataSetOk(dsParametros) && DataTableOk(dsParametros.Tables[0]) && dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"] + "" != "")
            nomeBancoSistemaOrcamento = dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"].ToString();

        string comandoSQL = string.Format(
            @"
                DECLARE @CodigoEntidade int,
                        @CodigoMovimentoOrcamento int,
                        @DespesaReceita char(1),
                        @CodigoEtapaMovimentoOrcamento int,
                        @Mes int
                        
                set @CodigoMovimentoOrcamento = {3}
                set @Mes = {4}

                SELECT @CodigoEntidade = IsNull(CodigoReservado,0)
                  FROM unidadenegocio 
                 WHERE CodigoEntidade = CodigoUnidadeNegocio 
                   AND SiglaUF = 'BR'
                   AND CodigoUnidadeNegocio = {6}                 


                SET @CodigoEtapaMovimentoOrcamento = (select max(CodigoEtapaMovimentoOrcamento) 
		                                                        from {2}.dbo.MovimentoOrcamentoEntidadeEtapa moee 
		                                                        WHERE moee.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento
		                                                          AND moee.CodigoEntidade = @CodigoEntidade
		                                                          AND moeE.CodigoStatusMovimentoOrcamento in (1,3) ) 

                DECLARE @DespesaPrevistaTotal numeric(18,2),
                        @ReceitaPrevistaTotal numeric(18,2),
                        @DespesaPrevistaData numeric(18,2),
                        @ReceitaPrevistaData numeric(18,2),
                        @DespesaReal numeric(18,2),
                        @ReceitaReal numeric(18,2)

                SELECT @DespesaPrevistaTotal = SUM(ValorOrcamento)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = @CodigoEtapaMovimentoOrcamento inner join
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'D'
                                                 
                SELECT @ReceitaPrevistaTotal = SUM(ValorOrcamento)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = @CodigoEtapaMovimentoOrcamento inner join
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'R'
                                                 
                SELECT @DespesaPrevistaData = SUM(ValorOrcamento)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = @CodigoEtapaMovimentoOrcamento 
                                                 AND op.Mes <=@Mes                                                       inner join
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'D'
                                                 
                SELECT @ReceitaPrevistaData = SUM(ValorOrcamento)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = @CodigoEtapaMovimentoOrcamento 
                                                 AND op.Mes <=@Mes                                                       inner join                                 
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'R'

                                                 
                -- A partir de 2011 a realização sempre fica no codigoEtapaMovimentoOrcamento = 1
                SELECT @DespesaReal = SUM(ValorReal)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = 1 --(case when @CodigoMovimentoOrcamento < 4 then 2 else 1 end) 
                                                 AND op.Mes <=@Mes    inner join
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'D'
                                                 
                SELECT @ReceitaReal = SUM(ValorReal)
                  FROM {2}.dbo.OrcamentoProjeto op inner join
                  
                       {2}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto
                                                 AND cop.DataExclusao is null 
                                                 AND cop.CodigoEtapaMovimentoOrcamento = 1 --(case when @CodigoMovimentoOrcamento < 4 then 2 else 1 end) 
                                                 AND op.Mes <=@Mes    inner join
                                                 
                       {2}.dbo.CR                        on CR.CodigoCR = cop.CodigoCR 
                                                 AND CR.DataExclusao is null
                                                 AND CR.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento inner join
                                                 
                       {2}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                                                 AND pco.CodigoEntidade = @CodigoEntidade
                                                 AND pco.DespesaReceita = 'R'
                                                 
                select ISNULL(@DespesaPrevistaTotal, 0) as DespesaPrevistaTotal,
                       ISNULL(@DespesaPrevistaData, 0) as DespesaPrevistaData, 
                       ISNULL(@DespesaReal, 0) as DespesaReal, 
                       ISNULL(@ReceitaPrevistaTotal, 0) as ReceitaPrevistaTotal, 
                       ISNULL(@ReceitaPrevistaData, 0) as ReceitaPrevistaData,
                       ISNULL(@ReceitaReal, 0) as ReceitaReal,
                       ISNULL(@ReceitaPrevistaTotal - @DespesaPrevistaTotal, 0) as ResultadoPrevistoTotal,
                       ISNULL(@ReceitaPrevistaData - @DespesaPrevistaData, 0) as ResultadoPrevistoData,  
                       ISNULL(@ReceitaReal - @DespesaReal, 0) as ResultadoReal
             ", bancodb, Ownerdb, nomeBancoSistemaOrcamento, orcamento, mes, where, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    public string getGraficoResultado(double med, double max, string valor, int raio, string linkOrcamento, bool exportar, int fonte, int margemInferior, string urlLink)
    {
        StringBuilder xml = new StringBuilder();

        int larguraBase = 5;
        int origemY = 140;

        if (raio > 100)
        {
            larguraBase = 10;
            origemY = 520;
        }

        string comandoExportar = "";

        if (exportar)
        {
            comandoExportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            comandoExportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        xml.Append(string.Format(@"
                    <Chart bgColor=""FFFFFF"" inDecimalSeparator=',' decimalSeparator=','  showValue=""1"" valueBelowPivot=""1"" chartBottomMargin=""{14}""
                                    {11} {13} 
                                    showBorder=""0"" showValue=""0"" fillAngle=""15"" upperLimit=""{1}"" lowerLimit=""-{1}"" baseFontSize=""{12}""
                                    displayValueDistance=""20"" gaugeInnerRadius=""0"" showGaugeBorder=""0"" formatNumberScale=""1"" 
                                    numberPrefix=""R$"" decimalPrecision=""2"" tickMarkDecimalPrecision=""0"" showPivotBorder=""1"" 
                                    >
	                    <colorRange>
		                    <color minValue=""-{1}"" maxValue=""0"" code=""{3}""/> 
		                    <color minValue=""0"" maxValue=""{0}"" code=""{4}""/>
		                    <color minValue=""{0}"" maxValue=""{1}"" code=""{5}""/>
	                    </colorRange>
		                    <dials>
		                    <dial value=""{2}"" borderAlpha=""0"" bgColor=""666666"" rearExtension='10' radius=""{7}""/>
	                    </dials>	
                    </Chart>", (int)med, (int)max / 3, valor, corCritico, corAtencao, corSatisfatorio, raio, raio, larguraBase, origemY, linkOrcamento, comandoExportar, fonte, urlLink, margemInferior));

        return xml.ToString();
    }

    public DataSet getMesesOrcamento(int codigoOrcamento, string where)
    {
        // Busca o nome do banco de dados do sistema de orçamento
        string nomeBancoSistemaOrcamento = "dbCdisOrcamento";
        DataSet dsParametros = getParametrosSistema("nomeBancoSistemaOrcamento");
        if (DataSetOk(dsParametros) && DataTableOk(dsParametros.Tables[0]) && dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"] + "" != "")
            nomeBancoSistemaOrcamento = dsParametros.Tables[0].Rows[0]["nomeBancoSistemaOrcamento"].ToString();

        //        string comandoSQL = string.Format(
        //             @"SELECT DISTINCT RIGHT('0' + CONVERT(varchar, roa.Mes), 2) AS Mes,
        //                       (CASE WHEN roa.Mes = 1 THEN 'JAN'  
        //		                     WHEN roa.Mes = 2 THEN 'FEV'  
        //		                     WHEN roa.Mes = 3 THEN 'MAR'  
        //		                     WHEN roa.Mes = 4 THEN 'ABR'  
        //		                     WHEN roa.Mes = 5 THEN 'MAI'  
        //		                     WHEN roa.Mes = 6 THEN 'JUN'  
        //		                     WHEN roa.Mes = 7 THEN 'JUL'  
        //		                     WHEN roa.Mes = 8 THEN 'AGO'  
        //		                     WHEN roa.Mes = 9 THEN 'SET'  
        //		                     WHEN roa.Mes = 10 THEN 'OUT' 
        //		                     WHEN roa.Mes = 11 THEN 'NOV'
        //		                     WHEN roa.Mes = 12 THEN 'DEZ' 
        //		                     ELSE 'Nenhum' END) AS nomeMes
        //                      FROM {0}.{1}.vi_ResumoOrcamentoAcao roa 
        //                     WHERE CodigoMovimentoOrcamento = {2} 
        //                       {3}
        //                     ORDER BY RIGHT('0' + CONVERT(varchar, roa.Mes), 2)
        //               ", bancodb, Ownerdb, codigoOrcamento, where);


        //        string comandoSQL = string.Format(
        //             @" DECLARE @mes TABLE (Mes char(2), NomeMes char(3))
        //                INSERT @mes values ('01', 'JAN') 
        //                INSERT @mes values ('02', 'FEV') 
        //                INSERT @mes values ('03', 'MAR') 
        //                INSERT @mes values ('04', 'ABR') 
        //                INSERT @mes values ('05', 'MAI') 
        //                INSERT @mes values ('06', 'JUN') 
        //                INSERT @mes values ('07', 'JUL') 
        //                INSERT @mes values ('08', 'AGO') 
        //                INSERT @mes values ('09', 'SET') 
        //                INSERT @mes values ('10', 'OUT') 
        //                INSERT @mes values ('11', 'NOV') 
        //                INSERT @mes values ('12', 'DEZ') 
        //
        //                SELECT * FROM @mes ORDER BY mes
        //               ", bancodb, Ownerdb, codigoOrcamento, where);


        string comandoSQL = string.Format(
             @" DECLARE @CodigoMovimentoOrcamento int
                SET @CodigoMovimentoOrcamento  = {1}

                SELECT DISTINCT RIGHT('0' + CONVERT(varchar, op.Mes), 2) AS Mes, 
                       (CASE WHEN op.Mes = 1 THEN 'JAN'  
		                     WHEN op.Mes = 2 THEN 'FEV'  
		                     WHEN op.Mes = 3 THEN 'MAR'  
		                     WHEN op.Mes = 4 THEN 'ABR'  
		                     WHEN op.Mes = 5 THEN 'MAI'  
		                     WHEN op.Mes = 6 THEN 'JUN'  
		                     WHEN op.Mes = 7 THEN 'JUL'  
		                     WHEN op.Mes = 8 THEN 'AGO'  
		                     WHEN op.Mes = 9 THEN 'SET'  
		                     WHEN op.Mes = 10 THEN 'OUT' 
		                     WHEN op.Mes = 11 THEN 'NOV'
		                     WHEN op.Mes = 12 THEN 'DEZ' 
		                     ELSE 'Nenhum' END) AS nomeMes
                      FROM {0}.dbo.OrcamentoProjeto op inner join
                           {0}.dbo.ContaOrcamentoProjeto cop on cop.CodigoContaProjeto = op.CodigoContaProjeto inner join
                           {0}.dbo.PlanoContasOrcamento pco on pco.CodigoConta = cop.CodigoConta
                     WHERE pco.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento
                       and CodigoEntidade = 5
                       and ValorReal <> 0
                       and cop.CodigoEtapaMovimentoOrcamento = 1
                       {2}
                     ORDER BY RIGHT('0' + CONVERT(varchar, op.Mes), 2)
               ", nomeBancoSistemaOrcamento, codigoOrcamento, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getMovimentoOrcamento(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT DISTINCT CodigoMovimentoOrcamento, DescricaoMovimentoOrcamento
                      FROM {0}.{1}.vi_MovimentoOrcamento
                     WHERE 1 = 1 {2}
                     ORDER BY DescricaoMovimentoOrcamento
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCodigoMovimentoOrcamentoAtual(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ISNULL(MAX(CodigoMovimentoOrcamento), -1) AS CodigoOrcamento
                      FROM {0}.{1}.vi_MovimentoOrcamento
                     WHERE 1 = 1 {2}
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCoresBulletsDesempenhoOrcamentoProjetos(int mes, int orcamento, string where, DataTable dtValoresOrcamento)
    {
        //decimal DespesaPrevistaData, decimal DespesaReal, decimal ReceitaPrevistaData, decimal ReceitaReal
        string DespesaPrevistaData, DespesaReal, ReceitaPrevistaData, ReceitaReal;
        DespesaPrevistaData = dtValoresOrcamento.Rows[0]["DespesaPrevistaData"].ToString().Replace(',', '.');
        DespesaReal = dtValoresOrcamento.Rows[0]["DespesaReal"].ToString().Replace(',', '.');
        ReceitaPrevistaData = dtValoresOrcamento.Rows[0]["ReceitaPrevistaData"].ToString().Replace(',', '.');
        ReceitaReal = dtValoresOrcamento.Rows[0]["ReceitaReal"].ToString().Replace(',', '.');

        string comandoSQL = string.Format(
             @"BEGIN
                      DECLARE @DesempenhoCusto Decimal(18,4),
                              @CustoLBData    Decimal(18,4),
                              @CustoRealData   Decimal(18,4),
                              @UltimoStatusCusto SmallInt,
                              @CorCusto        Varchar(30)

                      DECLARE @DesempenhoReceita Decimal(18,4),
                              @ReceitaLBData    Decimal(18,4),
                              @ReceitaRealData   Decimal(18,4),
                              @UltimoStatusReceita SmallInt,
                              @CorReceita        Varchar(30)

		              /* Parâmetros da consulta */
                      DECLARE @CodigoMovimentoOrcamento SmallInt,
                              @Mes SmallInt
	
			          SET @CodigoMovimentoOrcamento = {2}
		              SET @Mes = {3}

                      SET @CustoLBData     = {5}
                      SET @CustoRealData   = {6}
                      SET @ReceitaLBData   = {7}
                      SET @ReceitaRealData = {8}

                       /*  SELECT 
                                @CustoLBData = Sum(rp.DespesaPrevistaData),
                                @CustoRealData = Sum(rp.DespesaReal),
                                @ReceitaLBData = Sum(rp.ReceitaPrevistaData),
                                @ReceitaRealData = Sum(rp.ReceitaReal)
                           FROM {0}.{1}.vi_ResumoOrcamentoAcao AS rp INNER JOIN 
                                {0}.{1}.vi_MovimentoOrcamentoProjeto AS mop ON (mop.CodigoCR = rp.CodigoCR
									                                                     AND mop.CodigoMovimentoOrcamento = rp.CodigoMovimentoOrcamento)
                          WHERE rp.CodigoMovimentoOrcamento = @CodigoMovimentoOrcamento
                            AND rp.Mes = @Mes
                        */
                     
                         IF @CustoLBData = 0
                            SET @DesempenhoCusto = 0
                         ELSE
                            SET @DesempenhoCusto = @CustoRealData / @CustoLBData


                         SELECT TOP 1 @UltimoStatusCusto = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoCusto*100 >= par_ind.valorinicial
                            AND @DesempenhoCusto*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'FIN'          

                         SELECT @CorCusto = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusCusto


                         IF @ReceitaLBData = 0
                            SET @DesempenhoReceita = 0
                         ELSE
                            SET @DesempenhoReceita = @ReceitaRealData / @ReceitaLBData


                         SELECT TOP 1 @UltimoStatusReceita = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoReceita*100 >= par_ind.valorinicial
                            AND @DesempenhoReceita*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'REC'          

                         SELECT @CorReceita = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusReceita

                         SELECT @CorCusto AS CorFinanceiro, @CorReceita AS CorReceita

                    END
               ", bancodb, Ownerdb, orcamento, mes, where, DespesaPrevistaData, DespesaReal, ReceitaPrevistaData, ReceitaReal);
        return getDataSet(comandoSQL);
    }

    #region Exclusivo para Prefeitura de Belo Horizonte
    public DataSet getDotacoesOrcamentarias(int codigoEntidadeUsuarioResponsavel, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT Dotacao
                   ,DataExclusao
                   ,CodigoEntidade
             FROM {0}.{1}.pbh_DotacaoOrcamentaria
             WHERE DataExclusao is null and CodigoEntidade = {2} {3}
             ORDER BY Dotacao", getDbName(), getDbOwner(), codigoEntidadeUsuarioResponsavel, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiDotacaoOrcamentaria(string dotacao, int codigoEntidade, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
        INSERT INTO {0}.{1}.pbh_DotacaoOrcamentaria
           (Dotacao, DataExclusao, CodigoEntidade)
     VALUES(  '{2}',         null,{3})", getDbName(), getDbOwner(), dotacao, codigoEntidade);
            retorno = execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;

    }

    public bool atualizaDotacaoOrcamentaria(string dotacao, int codigoEntidade, string dotacaoSelecionada, ref string mensagemErro)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
        UPDATE {0}.{1}.pbh_DotacaoOrcamentaria
            SET Dotacao = '{2}'               
          WHERE CodigoEntidade = {3} AND Dotacao = '{4}'", getDbName(), getDbOwner(), dotacao, codigoEntidade, dotacaoSelecionada);
            retorno = execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiDotacaoOrcamentaria(string dotacaoSelecionada, int codigoEntidade, ref string mensagemErro)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        string comandoSQL = string.Format(@"
        UPDATE {0}.{1}.pbh_DotacaoOrcamentaria
           SET DataExclusao = getDate()
         WHERE CodigoEntidade = {2} AND Dotacao =  '{3}'", getDbName(), getDbOwner(), codigoEntidade, dotacaoSelecionada);
        try
        {
            retorno = execSQL(comandoSQL, ref registrosAfetados);
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

    #region Meu Dashboard

    public string getGraficoDesempenhoGerente(DataTable dt, int codigoGerente, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

            linkLista = (linkLista == true && podeAcessarLink == true);

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&CodigoGerente={0}&Programas=N&Branco=N"" ", codigoGerente);

            xmlAux.Append(string.Format(@"<set label=""Satisfatórios"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&CodigoGerente={0}&Programas=N&Branco=N"" ", codigoGerente);

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&CodigoGerente={0}&Programas=N&Branco=N"" ", codigoGerente);

            xmlAux.Append(string.Format(@"<set label=""Críticos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (mostraLaranja == "S")
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&CodigoGerente={0}&Programas=N&Branco=N"" ", codigoGerente);

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString(),
                                                                                                               "FF6600",
                                                                                                               link));
            }

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&CodigoGerente={0}&Programas=N&Branco=S"" ", codigoGerente);

            xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString()
                                                                                                         , "F3F3F3"
                                                                                                         , link));
        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" showZeroPies=\"0\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"   labelDistance=\"1\" enablesmartlabels=\"0\"" +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getRiscosQuestoesUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"/* Consulta para trazer a quantidade de riscos e questões vinculados ao usuário, separando em 
                       Críticos, Em Atenção e Sob Controle*/
                    BEGIN
                      /* Parâmetro da consulta */
                      DECLARE @CodigoUsuarioParam Int,
                              @CodigoEntidadeParam Int,
                              @TituloQuestoes Varchar(20)
                              
                      SET @CodigoUsuarioParam = {2}
                      SET @CodigoEntidadeParam = {3}
                       
                      SELECT @TituloQuestoes = Valor 
                        FROM {0}.{1}.ParametroConfiguracaoSistema
                       WHERE Parametro = 'labelQuestoes'                      
                      
                      SELECT CASE WHEN rq.IndicaRiscoQuestao = 'R' THEN 'Riscos' ELSE RTRIM(@TituloQuestoes) END AS Descricao,
                             ISNULL(Sum(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Verde' THEN 1 ELSE 0 END), 0) AS Verdes,
                             ISNULL(Sum(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Amarelo' THEN 1 ELSE 0 END), 0) AS Amarelos,
                             ISNULL(Sum(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END), 0) AS Vermelhos
                        FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN
							 {0}.{1}.Projeto p ON p.CodigoProjeto = rq.CodigoProjeto
                       WHERE CodigoUsuarioResponsavel = @CodigoUsuarioParam
                         AND rq.DataPublicacao IS NOT NULL
                         AND rq.DataExclusao IS NULL
                         AND rq.CodigoStatusRiscoQuestao IN ('QA','RA')
                         AND rq.CodigoEntidade = @CodigoEntidadeParam
                         {4}
                       GROUP BY CASE WHEN rq.IndicaRiscoQuestao = 'R' THEN 'Riscos' ELSE RTRIM(@TituloQuestoes) END
                          
                    END  
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoRiscosQuestoes(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");
            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Satisfatórios\"  color=\"" + corSatisfatorio + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                {
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_MeusRiscosProblemas.aspx?Cor=Verde&TipoTela=");

                    if (dt.Rows[i]["Descricao"].ToString() != "Riscos")
                    {
                        link += "Q\"";
                    }
                    else
                    {
                        link += "R\"";
                    }
                }

                xmlAux.Append(string.Format(@"<set label=""Satisfatórios"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Verdes"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Atenção\" color=\"" + corAtencao + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                {
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_MeusRiscosProblemas.aspx?Cor=Amarelo&TipoTela=");

                    if (dt.Rows[i]["Descricao"].ToString() != "Riscos")
                    {
                        link += "Q\"";
                    }
                    else
                    {
                        link += "R\"";
                    }
                }

                xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Amarelos"].ToString()
                                                                                                             , corAtencao
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Críticos\" color=\"" + corCritico + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                {
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_MeusRiscosProblemas.aspx?Cor=Vermelho&TipoTela=");

                    if (dt.Rows[i]["Descricao"].ToString() != "Riscos")
                    {
                        link += "Q\"";
                    }
                    else
                    {
                        link += "R\"";
                    }
                }

                xmlAux.Append(string.Format(@"<set label=""Críticos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Vermelhos"].ToString()
                                                                                                             , corCritico
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" showZeroPies=\"0\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"0\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getTarefasDashUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int,
                          @ResourceUID VarChar(64),
                          @DescricaoToDoList VarChar(25)
                          
                  SET @CodigoUsuarioParam = {2}
                  SET @CodigoEntidadeParam = {3}

                  SELECT @ResourceUID = CodigoRecursoMSProject 
                    FROM {0}.{1}.vi_RecursoCorporativo 
                   WHERE CodigoRecurso = @CodigoUsuarioParam

                 SELECT @DescricaoToDoList = valor 
                   FROM {0}.{1}.ParametroConfiguracaoSistema
                  WHERE Parametro = 'labelToDoList'
                    AND CodigoEntidade = {3}
                  
                  SELECT 'Cronograma' AS Descricao,
                         ISNULL(Sum(CASE WHEN t.TerminoLB < GetDate() THEN 1 ELSE 0 END), 0) AS TarefasAtrasadas,
                         ISNULL(SUM(CASE WHEN t.TerminoLB >= GetDate() THEN 1 ELSE 0 END), 0) AS TarefasFuturas
                    FROM {0}.{1}.vi_tarefa AS t INNER JOIN 
                         {0}.{1}.vi_Atribuicao AS a ON (a.CodigoTarefa = t.CodigoTarefa 
                                            AND a.CodigoProjeto = t.CodigoProjeto
                                            AND a.ResourceUID = @ResourceUID) INNER JOIN
                         {0}.{1}.Projeto AS p ON (p.CodigoProjeto = t.CodigoProjeto
                                      AND p.CodigoEntidade = @CodigoEntidadeParam)                   
                   WHERE t.IndicaTarefaSumario = 0
                     AND t.TerminoReal IS NULL
                   UNION
                     SELECT @DescricaoToDoList,
                             ISNULL(Sum(CASE WHEN t.TerminoPrevisto < GetDate() THEN 1 ELSE 0 END), 0),
                             ISNULL(Sum(CASE WHEN t.TerminoPrevisto >= GetDate() THEN 1 ELSE 0 END), 0)  
                        FROM {0}.{1}.TarefaToDoList AS t INNER JOIN
	  		                 {0}.{1}.ToDoList AS td ON (t.CodigoToDoList = td.CodigoToDoList
			                                AND td.DataExclusao IS NULL)
                      WHERE t.CodigoUsuarioResponsavelTarefa = @CodigoUsuarioParam
                        AND t.TerminoReal IS NULL
                        AND t.DataExclusao IS NULL
                        AND td.CodigoEntidade = {3}
                        AND t.CodigoStatusTarefa <> 3
                        {4}
                END
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoTarefas(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");
            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Futuras\"  color=\"" + corSatisfatorio + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                link = "";

                if (linkLista)
                {
                    if (dt.Rows[i]["Descricao"].ToString() != "Cronograma")
                        link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_MinhaTarefas.aspx?Estagio=Futura"" ");
                    else
                    {
                        link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_AtualizacaoTarefas.aspx"" ");
                        //link = string.Format(@"link=""F-_top-../../Tarefas/Atualizacao.aspx"" ");
                    }
                }

                xmlAux.Append(string.Format(@"<set label=""Futuras"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["TarefasFuturas"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Atrasadas\" color=\"" + corCritico + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                link = "";

                if (linkLista)
                {
                    if (dt.Rows[i]["Descricao"].ToString() != "Cronograma")
                        link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_MinhaTarefas.aspx?Estagio=Atrasada"" ");
                    else
                    {
                        link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_AtualizacaoTarefas.aspx"" ");
                        //link = string.Format(@"link=""F-_top-../../Tarefas/Atualizacao.aspx?Atrasadas=S"" ");
                    }
                }

                xmlAux.Append(string.Format(@"<set label=""Atrasadas"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["TarefasAtrasadas"].ToString()
                                                                                                             , corCritico
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"0\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getMensagensDashUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"/* Consulta para trazer a quantidade de mensagens do usuário (enviadas em projetos) que
                       ainda não foram lidas e também as que não foram respondidas, separando-as em atrasadas
                       e futuras */
                    BEGIN
                      DECLARE @CodigoUsuarioParam Int,
                              @CodigoEntidadeParam Int
                              
                      SET @CodigoUsuarioParam = {2}
                      SET @CodigoEntidadeParam = {3}
                        
                      SELECT CASE WHEN DataPrimeiroAcessoMensagem IS NULL THEN 'Não Lidas' ELSE 'Não Respondidas' END AS Descricao,
                             ISNULL(Sum(CASE WHEN DataPrimeiroAcessoMensagem IS NULL AND DataPrevistaAcaoMensagem IS NULL THEN 1 ELSE 0 END), 0) AS MensagensNaoLidas,
                             ISNULL(Sum(CASE WHEN DataPrevistaAcaoMensagem IS Not NULL AND DataPrevistaAcaoMensagem < GETDATE() THEN 1 ELSE 0 END), 0) AS MensagensNaoRespondidasAtrasadas,
                             ISNULL(Sum(CASE WHEN DataPrevistaAcaoMensagem IS Not NULL AND DataPrevistaAcaoMensagem >= GETDATE() THEN 1 ELSE 0 END), 0) AS MensagensNaoRespondidasFuturas
                        FROM {0}.{1}.CaixaMensagem AS cm INNER JOIN
                             {0}.{1}.TipoAssociacao AS ta ON (ta.CodigoTipoAssociacao = cm.CodigoTipoAssociacao
                                                  AND ta.IniciaisTipoAssociacao = 'MG')
                       WHERE cm.CodigoUsuario = @CodigoUsuarioParam
                         AND cm.CodigoEntidade = @CodigoEntidadeParam
                         AND cm.DataRetiradaMensagemCaixa IS NULL
                         AND cm.IndicaTipoMensagem = 'E'
                         {4}
                        GROUP BY CASE WHEN DataPrimeiroAcessoMensagem IS NULL THEN 'Não Lidas' ELSE 'Não Respondidas' END
                      
                    END
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoMensagens(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");
            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Futuras\"  color=\"" + corSatisfatorio + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_CaixaEntrada.aspx?CTA=MG&Status=NL"" ");

                xmlAux.Append(string.Format(@"<set label=""Futuras"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["MensagensNaoLidas"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Futuras\"  color=\"" + corSatisfatorio + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_CaixaEntrada.aspx?CTA=MG&Status=NRF"" ");

                xmlAux.Append(string.Format(@"<set label=""Futuras"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["MensagensNaoRespondidasFuturas"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Atrasadas\" color=\"" + corCritico + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_CaixaEntrada.aspx?CTA=MG&Status=NRA"" ");

                xmlAux.Append(string.Format(@"<set label=""Atrasadas"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["MensagensNaoRespondidasAtrasadas"].ToString()
                                                                                                             , corCritico
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"0\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getAlocacaoDashUsuario(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int
                          
                  SET @CodigoUsuarioParam = {2}
                  SET @CodigoEntidadeParam = {3}
                  
                  DECLARE @tblDisponibilidade TABLE
                    (Data DateTime,
                     Disponibilidade Decimal(18,2))
                  
                  DECLARE @tblAlocacao TABLE
                     (Data DateTime,
                      Alocacao Decimal(18,2))  
                  
                  /* Busca a Disponibilidade do recurso para os próximos 30 dias */
                  INSERT INTO @tblDisponibilidade
                  SELECT ddr.Data,
                         SUM(ddr.CapacidadeAlocacaoRecurso)
                    FROM {0}.{1}.CapacidadeAlocacaoDiariaRecurso AS ddr INNER JOIN
                         {0}.{1}.RecursoCorporativo AS rc ON (rc.CodigoUsuario = @CodigoUsuarioParam
                                                     AND ddr.CodigoRecursoCorporativo = rc.CodigoRecursoCorporativo)
                   WHERE ddr.Data >= GETDATE()
                     AND ddr.Data <= GETDATE()+7
                     AND DATEPART(WEEKDAY,ddr.Data) NOT IN (1,7)
                     AND rc.CodigoEntidade = @CodigoEntidadeParam
                  GROUP BY ddr.Data

                  
                  
                  /* Busca a alocacao do recurso para os próximos 30 dias */
                  INSERT INTO @tblAlocacao
                  SELECT adr.Data,
                         SUM(adr.Trabalho)
                    FROM {0}.{1}.vi_AtribuicaoDiariaRecurso AS adr INNER JOIN
                         {0}.{1}.vi_RecursoCorporativo AS rc ON (rc.CodigoRecurso = @CodigoUsuarioParam
                                                     AND adr.ResourceUID = rc.CodigoRecursoMSProject)
                   WHERE adr.Data >= GETDATE()
                     AND adr.Data <= GETDATE()+7   
                     AND DATEPART(WEEKDAY,adr.Data) NOT IN (1,7)
                     AND rc.CodigoEntidade = @CodigoEntidadeParam
                  GROUP BY adr.Data
                  
                  SELECT CONVERT(Char(10), d.Data, 103) AS Data,
                         SUM(IsNull(a.Alocacao,0)) AS Alocacao,
                         SUM(IsNull(d.Disponibilidade,0)) AS Disponibilidade
                    FROM @tblDisponibilidade AS d LEFT JOIN
                         @tblAlocacao AS a ON (a.Data = d.Data) 
                  GROUP BY CONVERT(Char(10), d.Data, 103), d.Data
                  ORDER BY d.Data          
                END
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoAlocacao(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Data"] + "\"/>");
            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Alocação\" >");

            string link = "";

            if (linkLista)
            {
                //                link = string.Format(@"link=""F-_top-../../espacoTrabalho/frameEspacoTrabalho_AtualizacaoTarefas.aspx"" ");
                link = string.Format(@"link=""F-_top-../../Tarefas/Atualizacao.aspx"" ");
            }

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {

                xmlAux.Append(string.Format(@"<set label=""Alocação"" value=""{0}"" {2}/>", dt.Rows[i]["Alocacao"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset renderAs=\"Line\" seriesName=\"Capacidade\" >");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Capacidade"" value=""{0}"" {2}/>", dt.Rows[i]["Disponibilidade"].ToString()
                                                                                                             , corCritico
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"1\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"2\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\" inDecimalSeparator=\",\" decimalSeparator=\",\" " +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getAcoesDashUsuario(int codigoUsuario, int codigoEntidade, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int,
                          @Aprovacoes Int,
		                  @Reunioes Int,
		                  @Indicadores Int,
                          @Avisos Int,
                          @CodigoCarteira Int
                          
                  SET @CodigoUsuarioParam = {2}
                  SET @CodigoEntidadeParam = {3}
                  Set @CodigoCarteira = {4}   
                  
                  SELECT @Aprovacoes = COUNT(1)
                    FROM {0}.{1}.CaixaMensagem AS cm INNER JOIN
                         {0}.{1}.TipoAssociacao AS ta ON (ta.CodigoTipoAssociacao = cm.CodigoTipoAssociacao
                                              AND ta.IniciaisTipoAssociacao = 'NF')
                   WHERE cm.CodigoUsuario = @CodigoUsuarioParam
                     AND cm.CodigoEntidade = @CodigoEntidadeParam
                     AND cm.DataRetiradaMensagemCaixa IS NULL
                     AND cm.IndicaTipoMensagem = 'E'
                  
                  SELECT @Indicadores = COUNT(1)
                    FROM {0}.{1}.IndicadorUnidade AS iu
                   WHERE iu.CodigoResponsavelIndicadorUnidade = @CodigoUsuarioParam
                     AND iu.CodigoUnidadeNegocio = @CodigoEntidadeParam
                     AND iu.DataExclusao IS NULL
                  
                  SELECT @Reunioes = COUNT(DISTINCT pe.CodigoEvento)
                    FROM {0}.{1}.ParticipanteEvento AS pe INNER JOIN
                         {0}.{1}.Evento AS e ON (e.CodigoEvento = pe.CodigoEvento
                                     AND e.CodigoEntidade = @CodigoEntidadeParam
                                     AND ((Convert(Varchar,e.InicioPrevisto,112) >= Convert(Varchar,GETDATE(),112) AND Convert(Varchar,e.InicioPrevisto,112) < Convert(Varchar,GETDATE()+7,112))
                                     AND (Convert(Varchar,e.TerminoPrevisto,112) >= Convert(Varchar,GETDATE(),112) AND Convert(Varchar,e.TerminoPrevisto,112) < Convert(Varchar,GETDATE()+7,112))))
                   WHERE pe.CodigoParticipante = @CodigoUsuarioParam
                      OR e.CodigoResponsavelEvento = @CodigoUsuarioParam
                     -- AND pe.DataConfirmacaoParticipante IS NOT NULL -- Esta cláusula será incluída somente quando a opção para confirmar participação estiver desenvolvida.
                    
                   
                 
                  SELECT @Avisos = COUNT(1)
                   FROM Aviso AS av 
                  WHERE GetDate() Between av.DataInicio AND av.DataTermino
                    AND av.CodigoEntidade = @CodigoEntidadeParam
                    AND NOT EXISTS(SELECT 1
                                     FROM AvisoDestinatario AS ad INNER JOIN
                                          AvisoLido AS al ON (al.CodigoAviso = av.CodigoAviso
                                                          AND al.CodigoUsuario = @CodigoUsuarioParam))
                                                      
                    AND av.CodigoAviso IN  (SELECT a.CodigoAviso
														  FROM {0}.{1}.Aviso a INNER JOIN 
																	 {0}.{1}.AvisoDestinatario ad ON ad.CodigoAviso = a.CodigoAviso INNER JOIN
																	 {0}.{1}.usuario psu ON psu.CodigoUsuario = ad.CodigoDestinatario LEFT JOIN
																	 {0}.{1}.AvisoLido al ON al.CodigoAviso = a.CodigoAviso AND al.CodigoUsuario = @CodigoUsuarioParam
														 WHERE ad.tipoDestinatario = 'US'
															 AND psu.CodigoUsuario = @CodigoUsuarioParam
															 AND a.CodigoEntidade = @CodigoEntidadeParam																 
														 UNION
														SELECT a.CodigoAviso
															FROM {0}.{1}.Aviso a INNER JOIN 
																	 {0}.{1}.AvisoDestinatario ad ON ad.CodigoAviso = a.CodigoAviso INNER JOIN
																	 {0}.{1}.f_GetProjetosUsuario({2}, {3}, {4}) gpr ON gpr.CodigoProjeto = ad.CodigoDestinatario LEFT JOIN
																	 {0}.{1}.AvisoLido al ON al.CodigoAviso = a.CodigoAviso  AND al.CodigoUsuario = @CodigoUsuarioParam
															 WHERE ad.tipoDestinatario = 'PR'																 
															 UNION
														SELECT a.CodigoAviso
															FROM {0}.{1}.Aviso a INNER JOIN 
																	 {0}.{1}.AvisoDestinatario ad ON ad.CodigoAviso = a.CodigoAviso INNER JOIN
																	 {0}.{1}.f_GetUnidadesUsuario({2},{3}) guu ON guu.CodigoUnidadeNegocio = ad.CodigoDestinatario LEFT JOIN
																	 {0}.{1}.AvisoLido al ON al.CodigoAviso = a.CodigoAviso  AND al.CodigoUsuario = @CodigoUsuarioParam
														 WHERE ad.tipoDestinatario = 'UN'																 
															UNION
														SELECT a.CodigoAviso
															FROM {0}.{1}.Aviso a INNER JOIN 
																	 {0}.{1}.AvisoDestinatario ad ON ad.CodigoAviso = a.CodigoAviso LEFT JOIN
																	 {0}.{1}.AvisoLido al ON al.CodigoAviso = a.CodigoAviso  AND al.CodigoUsuario = @CodigoUsuarioParam
															 WHERE ad.tipoDestinatario = 'TD')     

                  SELECT ISNULL(@Reunioes, 0) AS Reunioes, ISNULL(@Aprovacoes, 0) AS Aprovacoes, ISNULL(@Indicadores, 0) AS Indicadores, IsNull(@Avisos,0) AS Avisos             
 

                END
               ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Painel de Demandas

    public DataSet getQuantidadeProjetosDemandas(int codigoEntidade, int codigoUsuario, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"
         BEGIN
	        DECLARE @DemandasSimples int,
			        @DemandasComplexas int,
			        @Projetos int,
                    @Processos int,
			        @CodigoUsuario int,
			        @codigoEntidade int,
                    @CodigoCarteira int
        			
	        SET @codigoEntidade = {2}
	        SET @CodigoUsuario = {3}
            SET @CodigoCarteira = {4}
        			
	        SELECT @DemandasSimples = COUNT(1) 
	          FROM {0}.{1}.Projeto p INNER JOIN
	               {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.CodigoProjeto = p.CodigoProjeto INNER JOIN 
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                       AND s.IndicaAcompanhamentoExecucao = 'S')                            
	         WHERE p.CodigoTipoProjeto = 5
	           AND p.DataExclusao IS NULL
               {5}
        	   
	        SELECT @DemandasComplexas = COUNT(1) 
	          FROM {0}.{1}.Projeto p INNER JOIN
	               {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.CodigoProjeto = p.CodigoProjeto INNER JOIN 
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                       AND s.IndicaAcompanhamentoExecucao = 'S')                            
	         WHERE p.CodigoTipoProjeto = 4
	           AND p.DataExclusao IS NULL
               {5}
        	   
	        SELECT @Projetos = Count(1)
              FROM {0}.{1}.Projeto p INNER JOIN 
		           {0}.{1}.f_GetProjetosUsuario( @CodigoUsuario, @codigoEntidade, @CodigoCarteira ) F ON (F.CodigoProjeto = p.CodigoProjeto) INNER JOIN 
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                       AND s.IndicaAcompanhamentoExecucao = 'S')                            
             WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ') 
               AND p.CodigoStatusProjeto = 3
               {5}

            SELECT @Processos = Count(1)
              FROM {0}.{1}.Projeto p INNER JOIN 
		           {0}.{1}.f_GetProcessosUsuario( @CodigoUsuario, @codigoEntidade ) F ON (F.CodigoProjeto = p.CodigoProjeto) INNER JOIN 
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                       AND s.IndicaAcompanhamentoExecucao = 'S')                            
             WHERE p.CodigoTipoProjeto = 3 
               AND p.DataExclusao IS NULL
               {5}
        	   
	        SELECT @DemandasSimples AS DemandasSimples, @DemandasComplexas AS DemandasComplexas, @Projetos AS Projetos, @Processos AS Processos
         
        END
        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoProjetosXDemandas(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

            incluiLink = (incluiLink == true && podeAcessarLink == true);

            if (incluiLink)
                link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DS"" ");

            xmlAux.Append(string.Format(@"<set label=""Demandas Simples"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasSimples"].ToString(),
                                                                                                           corDemandasSimples,
                                                                                                           link));

            if (incluiLink)
                link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DC"" ");

            xmlAux.Append(string.Format(@"<set label=""Demandas Complexas"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasComplexas"].ToString(),
                                                                                                           corDemandasComplexas,
                                                                                                           link));

            if (incluiLink)
                link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Programas=N"" ");

            xmlAux.Append(string.Format(@"<set label=""Projetos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Projetos"].ToString(),
                                                                                                           corProjetos,
                                                                                                           link));

            if (incluiLink)
                link = string.Format(@"link=""F-_top-../resumoProcessos.aspx"" ");

            xmlAux.Append(string.Format(@"<set label=""Processos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Processos"].ToString(),
                                                                                                           corProcessos,
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\" labelDistance=\"1\" enablesmartlabels=\"0\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\"" +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getQuantidadeDemandasPorStatus(int codigoEntidade, int codigoUsuario, string where)
    {
        comandoSQL = string.Format(@"
                        SELECT s.CodigoStatus, s.DescricaoStatus, COUNT(1) AS Quantidade
                          FROM {0}.{1}.Projeto p INNER JOIN
	                           {0}.{1}.f_GetDemandasUsuario({3}, {2}) f ON f.CodigoProjeto = p.CodigoProjeto INNER JOIN
	                           {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusProjeto
                         WHERE 1 = 1 
                           {4}
                         GROUP BY s.CodigoStatus, s.DescricaoStatus
        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDemandasPorStatus(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;

        try
        {
            foreach (DataRow dr in dt.Rows)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?StatusDemanda={0}"" ", dr["CodigoStatus"].ToString());

                xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" {2}/>", dr["DescricaoStatus"].ToString(),
                                                                                       dr["Quantidade"].ToString(),
                                                                                       link));
            }

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\" showZeroPies=\"0\" labelDistance=\"1\" enablesmartlabels=\"0\" slantLabels=\"1\" labelDisplay=\"ROTATE\"" +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getQuantidadeProjetosDemandasPorUnidade(int codigoEntidade, int codigoUsuario, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"
         BEGIN
	        DECLARE @DemandasSimples int,
			        @DemandasComplexas int,
			        @Projetos int,
			        @CodigoUsuario int,
			        @codigoEntidade int,
                    @CodigoCarteira int 
        			
	        SET @codigoEntidade = {2}
	        SET @CodigoUsuario = {3}
            SET @CodigoCarteira = {4}
	        
			SELECT u.CodigoUnidadeNegocio AS Codigo, u.SiglaUnidadeNegocio AS Descricao,
								(SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
													  {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                                      {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                    AND s.IndicaAcompanhamentoExecucao = 'S')
												WHERE p.CodigoTipoProjeto = 5
												  AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS DemandasSimples,
								(SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
													  {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                                      {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                    AND s.IndicaAcompanhamentoExecucao = 'S')
												WHERE p.CodigoTipoProjeto = 4
												  AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS DemandasComplexas,
								(SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
													  {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @codigoEntidade, @CodigoCarteira) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
													  {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
																AND s.IndicaAcompanhamentoExecucao = 'S')                     
												WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
												   AND p.CodigoStatusProjeto = 3 
								   AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS Projetos,
								(SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
													  {0}.{1}.f_GetProcessosUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                                      {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                    AND s.IndicaAcompanhamentoExecucao = 'S')
												WHERE p.CodigoTipoProjeto = 3
												  AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS Processos
							  FROM {0}.{1}.UnidadeNegocio u
							 WHERE u.CodigoUnidadeNegocio IN (SELECT p.CodigoUnidadeNegocio 
																FROM {0}.{1}.Projeto p INNER JOIN
																	 {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @codigoEntidade, @CodigoCarteira) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
																	 {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
																										  AND s.IndicaAcompanhamentoExecucao = 'S')                     
															   WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
																 AND p.CodigoStatusProjeto = 3
									                              UNION
                                                                  SELECT p.CodigoUnidadeNegocio 
																	FROM {0}.{1}.Projeto p INNER JOIN
																		 {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto  INNER JOIN 
                                                                          {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                                        AND s.IndicaAcompanhamentoExecucao = 'S')
                                                                  UNION
                                                                  SELECT p.CodigoUnidadeNegocio 
																	FROM {0}.{1}.Projeto p INNER JOIN
																		 {0}.{1}.f_GetProcessosUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto    INNER JOIN 
                                                                          {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                                                                                        AND s.IndicaAcompanhamentoExecucao = 'S')         
																  )
																  
        END
        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoProjetosDemandasPorUnidade(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {
            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Demandas Simples\"  color=\"" + corDemandasSimples + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DS"" ");

                xmlAux.Append(string.Format(@"<set label=""Demandas Simples"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasSimples"].ToString()
                                                                                                             , corDemandasSimples
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Demandas Complexas\" color=\"" + corDemandasComplexas + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DC"" ");

                xmlAux.Append(string.Format(@"<set label=""Demandas Complexas"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasComplexas"].ToString()
                                                                                                             , corDemandasComplexas
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Projetos\" color=\"" + corProjetos + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Programas=N"" ");

                xmlAux.Append(string.Format(@"<set label=""Projetos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Projetos"].ToString()
                                                                                                             , corProjetos
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Processos\"  color=\"" + corProcessos + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoProcessos.aspx"" ");

                xmlAux.Append(string.Format(@"<set label=""Processos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Processos"].ToString()
                                                                                                             , corProcessos
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"0\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getAlocacaoRecursosProjetosDemandas(int codigoEntidade, int codigoUsuario, string dataParametro, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"
         /* 
         Objetivos..: Trazer as informações necessárias para preenchimento do gráfico comparativo 
                                         da alocação por projeto, demandas simples e demandas complexas.
         Parâmetro..: Data utilizada para delimitar o início de apuração das horas para apresentar no
                                             gráfico.
        */                                                                  
        BEGIN
          DECLARE @CodigoEntidadeParam Int,
                  @DataParam DateTime,
                  @CodigoUsuarioParam Int,
                  @CodigoCarteira Int
                  
          SET @CodigoEntidadeParam = {2} 
          SET @DataParam = CONVERT(DateTime, '{4}', 103)
          SET @CodigoUsuarioParam = {3}
          SET @CodigoCarteira = {5}
         
          
          SELECT CASE tp.IndicaTipoProjeto WHEN 'PRJ' THEN 'Projetos'     
                      WHEN 'PRC' THEN 'Processos'
                      WHEN 'DMC' THEN 'Demandas Complexas'
                      WHEN 'DMS' THEN 'Demandas Simples'
                      ELSE '' END AS TipoProjeto,
                 SUM(IsNull(tsp.TrabalhoAprovado,0)) AS Alocacao,
                 tp.CodigoTipoProjeto
            FROM {0}.{1}.Projeto AS p INNER JOIN
                 {0}.{1}.TimeSheetProjeto AS tsp ON (tsp.CodigoProjeto = p.CodigoProjeto
                                         AND tsp.StatusTimesheet = 'AP') INNER JOIN
                 {0}.{1}.TipoProjeto AS tp ON (tp.CodigoTipoProjeto = p.CodigoTipoProjeto)                        
           WHERE p.DataExclusao IS NULL
             AND p.CodigoEntidade = @CodigoEntidadeParam      
             AND tsp.Data >= @DataParam
             AND p.CodigoProjeto IN (SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetProjetosUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam, @CodigoCarteira)
                                      UNION
                                     SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetDemandasUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam)
                                     UNION
                                     SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetProcessosUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam))
             AND p.CodigoTipoProjeto < 6 -- considerar todos os tipos de projetos com exceção do 'horas não trabalhadas'
           GROUP BY tp.TipoProjeto, tp.CodigoTipoProjeto, tp.IndicaTipoProjeto
        END
        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, dataParametro, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoAlocacaoRecursosProjetosDemandas(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;

        try
        {
            foreach (DataRow dr in dt.Rows)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx"" ");

                string cor = "";

                switch (int.Parse(dr["CodigoTipoProjeto"].ToString()))
                {
                    case 1:
                        cor = corProjetos;
                        break;
                    case 4:
                        cor = corDemandasComplexas;
                        break;
                    case 5:
                        cor = corDemandasSimples;
                        break;
                    default:
                        cor = "FFFFFF";
                        break;
                }

                xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" color=""{2}"" {3}/>", dr["TipoProjeto"].ToString(),
                                                                                       dr["Alocacao"].ToString(),
                                                                                       cor,
                                                                                       link));
            }

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" thousandSeparator=\".\" inDecimalSeparator=\",\" decimalSeparator=\",\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"1\" baseFontSize=\"" + fonte + "\">");

        xml.Append(xmlAux.ToString());
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

    public DataSet getAlocacaoRecursosProjetosDemandasUnidadesAnterior(int codigoEntidade, int codigoUsuario, string dataParametro, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"
         /* 
         Objetivos..: Trazer a alocação de recursos por tipo (projeto, demanda) por unidade de negócio para composição
                      do gráfico de colunas superpostas intitulado Alocação de Recursos por Projeto, Demanda e Unidade
         Parâmetros.: Data utilizada para delimitar o início de apuração das horas para apresentar no
                                             gráfico
        */         
         BEGIN
            DECLARE @DemandasSimples int,
	                @DemandasComplexas int,
	                @Projetos int,
	                @CodigoUsuario int,
                    @DataParam DateTime,
	                @codigoEntidade int,
                    @CodigoCarteira Int
        			
            SET @codigoEntidade = {2}
            SET @CodigoUsuario = {3}
            SET @DataParam = CONVERT(DateTime, '{4}', 103)
            SET @CodigoCarteira = {5}
            
	        SELECT u.CodigoUnidadeNegocio AS Codigo, u.SiglaUnidadeNegocio AS Descricao,
		            (SELECT SUM(IsNull(t.TrabalhoAprovado,0))
			            FROM {0}.{1}.Projeto AS p INNER JOIN
				             {0}.{1}.TimeSheetProjeto AS t ON (t.CodigoProjeto = p.CodigoProjeto 
										             AND t.StatusTimesheet = 'AP')INNER JOIN
				             {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto                        
		               WHERE p.DataExclusao IS NULL    
			             AND t.Data >= @DataParam
			             AND p.CodigoTipoProjeto = 5
		                 AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS DemandasSimples,
		            (SELECT SUM(IsNull(t.TrabalhoAprovado,0))
			            FROM {0}.{1}.Projeto AS p INNER JOIN
				             {0}.{1}.TimeSheetProjeto AS t ON (t.CodigoProjeto = p.CodigoProjeto 
										             AND t.StatusTimesheet = 'AP')INNER JOIN
				             {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto                        
		               WHERE p.DataExclusao IS NULL    
			             AND t.Data >= @DataParam
			             AND p.CodigoTipoProjeto = 4
		                 AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS DemandasComplexas,
		             (SELECT SUM(IsNull(t.TrabalhoAprovado,0))
			            FROM {0}.{1}.Projeto AS p INNER JOIN
				             {0}.{1}.TimeSheetProjeto AS t ON (t.CodigoProjeto = p.CodigoProjeto 
										             AND t.StatusTimesheet = 'AP')INNER JOIN
				             {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @codigoEntidade, @CodigoCarteira) f ON f.codigoProjeto = p.CodigoProjeto                        
		               WHERE p.DataExclusao IS NULL    
			             AND t.Data >= @DataParam
			             AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
		                 AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS Projetos,
		            (SELECT SUM(IsNull(t.TrabalhoAprovado,0))
			            FROM {0}.{1}.Projeto AS p INNER JOIN
				             {0}.{1}.TimeSheetProjeto AS t ON (t.CodigoProjeto = p.CodigoProjeto 
										             AND t.StatusTimesheet = 'AP')INNER JOIN
				             {0}.{1}.f_GetProcessosUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto                        
		               WHERE p.DataExclusao IS NULL    
			             AND t.Data >= @DataParam
			             AND p.CodigoTipoProjeto = 3
		                 AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) AS Processos
	          FROM {0}.{1}.UnidadeNegocio u
	         WHERE u.CodigoUnidadeNegocio IN (SELECT p.CodigoUnidadeNegocio 
										        FROM {0}.{1}.Projeto p INNER JOIN
											         {0}.{1}.TimeSheetProjeto AS tsp ON (tsp.CodigoProjeto = p.CodigoProjeto
																					 AND tsp.StatusTimesheet = 'AP') INNER JOIN
											         {0}.{1}.f_GetProjetosUsuario(@CodigoUsuario, @codigoEntidade, @CodigoCarteira) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
											         {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto)                     
									           WHERE p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
			                                  UNION
                                              SELECT p.CodigoUnidadeNegocio 
											    FROM {0}.{1}.Projeto p INNER JOIN
													 {0}.{1}.TimeSheetProjeto AS tsp ON (tsp.CodigoProjeto = p.CodigoProjeto
																					 AND tsp.StatusTimesheet = 'AP') INNER JOIN
												     {0}.{1}.f_GetDemandasUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto
                                             UNION
                                              SELECT p.CodigoUnidadeNegocio 
											    FROM {0}.{1}.Projeto p INNER JOIN
													 {0}.{1}.TimeSheetProjeto AS tsp ON (tsp.CodigoProjeto = p.CodigoProjeto
																					 AND tsp.StatusTimesheet = 'AP') INNER JOIN
												     {0}.{1}.f_GetProcessosUsuario(@CodigoUsuario, @codigoEntidade) f ON f.codigoProjeto = p.CodigoProjeto)
        														  
                END

        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, dataParametro, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoAlocacaoRecursosProjetosDemandasUnidadesAnterior(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

            incluiLink = (incluiLink == true && podeAcessarLink == true);

            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Demandas Simples\"  color=\"" + corDemandasSimples + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DS"" ");

                xmlAux.Append(string.Format(@"<set label=""Demandas Simples"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasSimples"].ToString()
                                                                                                             , corDemandasSimples
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Demandas Complexas\" color=\"" + corDemandasComplexas + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx?Tipo=DC"" ");

                xmlAux.Append(string.Format(@"<set label=""Demandas Complexas"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["DemandasComplexas"].ToString()
                                                                                                             , corDemandasComplexas
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Projetos\" color=\"" + corProjetos + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Programas=N"" ");

                xmlAux.Append(string.Format(@"<set label=""Projetos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Projetos"].ToString()
                                                                                                             , corProjetos
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Processos\" color=\"" + corProjetos + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoProcessos.aspx"" ");

                xmlAux.Append(string.Format(@"<set label=""Processos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Processos"].ToString()
                                                                                                             , corProcessos
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");
        }
        catch
        {
            return "";
        }

        float qtdProjetos = float.Parse(i.ToString()) / 2;

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"6\"  scrollHeight=\"12\"  showLegend=\"0\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" showvalues=\"0\" decimals=\"1\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"5\" chartRightMargin=\"8\" chartBottomMargin=\"2\" chartLeftMargin=\"4\" inThousandSeparator=\".\" thousandSeparator=\".\" inDecimalSeparator=\",\" decimalSeparator=\",\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" slantLabels=\"1\" labelDisplay=\"ROTATE\"  decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getAlocacaoRecursosTiposProjetosDemandas(int codigoEntidade, int codigoUsuario, string dataParametro, int codigoCarteira, string where)
    {
        comandoSQL = string.Format(@"
         /* 
         Objetivos..: Trazer as informações necessárias para preenchimento do gráfico comparativo 
                                         da alocação por projeto, demandas simples e demandas complexas.
         Parâmetro..: Data utilizada para delimitar o início de apuração das horas para apresentar no
                                             gráfico.
        */                                                                  
        BEGIN
          DECLARE @CodigoEntidadeParam Int,
                  @DataParam DateTime,
                  @CodigoUsuarioParam Int,
                  @CodigoCarteira Int
                  
          SET @CodigoEntidadeParam = {2} 
          SET @DataParam = CONVERT(DateTime, '{4}', 103)
          SET @CodigoUsuarioParam = {3}
          Set @CodigoCarteira = {5}
         
          
          SELECT ttts.DescricaoTipoTarefaTimeSheet AS Descricao,
                 SUM(IsNull(tsp.TrabalhoAprovado,0)) AS Alocacao
            FROM {0}.{1}.Projeto AS p INNER JOIN
                 {0}.{1}.TimeSheetProjeto AS tsp ON (tsp.CodigoProjeto = p.CodigoProjeto
                                         AND tsp.StatusTimesheet = 'AP') INNER JOIN
                 {0}.{1}.TipoTarefaTimeSheet AS ttts ON (ttts.CodigoTipoTarefaTimeSheet = tsp.CodigoTipoTarefaTimeSheet)                        
           WHERE p.DataExclusao IS NULL
             AND p.CodigoEntidade = @CodigoEntidadeParam   
             AND ttts.CodigoEntidade = @CodigoEntidadeParam   
             AND tsp.Data >= @DataParam
             AND p.CodigoProjeto IN (SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetProjetosUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam, @CodigoCarteira)
                                      UNION
                                     SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetDemandasUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam)
                                      UNION
                                     SELECT CodigoProjeto
                                       FROM {0}.{1}.f_GetProcessosUsuario(@CodigoUsuarioParam,@CodigoEntidadeParam)) 
             AND p.CodigoTipoProjeto < 6 -- considerar todos os tipos de projetos com exceção do 'horas não trabalhadas'
           GROUP BY ttts.DescricaoTipoTarefaTimeSheet
        END
        ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, dataParametro, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoAlocacaoRecursosTiposProjetosDemandas(DataTable dt, string titulo, int fonte, bool incluiLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;

        try
        {
            foreach (DataRow dr in dt.Rows)
            {
                if (incluiLink)
                    link = string.Format(@"link=""F-_top-../resumoDemandas.aspx"" ");

                xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" {2}/>", dr["Descricao"].ToString(),
                                                                                       dr["Alocacao"].ToString(),
                                                                                       link));
            }

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDistance=\"1\" enablesmartlabels=\"0\" labelDisplay=\"ROTATE\" thousandSeparator=\".\" inDecimalSeparator=\",\" decimalSeparator=\",\" " +
            " showValues=\"1\" showLegend=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"1\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    #endregion

    #region Painel de Estágios
    public DataSet SME_getOLAPEstagio1(int codigoUsuario, int? codigoEntidade, string dataInicial, string dataFinal)
    {
        //dados forçados para configuração do relatorio de demandas

        string strCodigoEntidade = codigoEntidade.HasValue ? codigoEntidade.ToString() : "NULL";

        string comandoSQL = string.Format(@"
        BEGIN        
            DECLARE @DataInicial    SmallDateTime
            DECLARE @DataFinal      SmallDateTime
            SET @DataInicial        = CONVERT(SmallDateTime, '{4}', 103)
            SET @DataFinal          = CONVERT(SmallDateTime, '{5}', 103)

            SELECT
			      [CodigoEntidade]								
			    , [CodigoEntidade]								
			    , [NomeEntidade]									
			    , [SiglaUF]												
			    , [Regiao]												
			    , [NomeIndicador]									
			    , [DescricaoDado]									
                , [Ano]						
                , [Mes]						
                , [Trimestre]			
                , [Semestre]			
			    , [ValorDado]											
            FROM {0}.{1}.[f_sme_GetDadosEstagioIEL_OLAP1]({2}, {3}, @DataInicial, @DataFinal)
        END", bancodb, Ownerdb, codigoUsuario, strCodigoEntidade, dataInicial, dataFinal);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    #endregion

    #region Estágios

    public DataSet getPrimeiroAnoAtivoEntidade(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
                                SELECT MIN(Ano) AS Ano 
                                  FROM {0}.{1}.PeriodoEstrategia
                                 WHERE CodigoUnidadeNegocio = {2}
                                   AND IndicaAnoAtivo = 'S'
                                   {3}", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getComparativosVagasEstagios(int codigoEntidade, int codigoUsuarioLogado, string dataInicio, string dataTermino)
    {
        comandoSQL = string.Format(@"
                              BEGIN
                                  DECLARE @d1 smalldatetime, @d2 smalldatetime

                                  SET 	@d1 = CONVERT(SmallDateTime, '{4}', 103)
                                  SET 	@d2 = CONVERT(SmallDateTime, '{5}', 103)

                                  SELECT f1.QuantidadeVagasPreenchidas, f1.QuantidadeVagasNaoPreenchidas 
                                    FROM {0}.{1}.f_sme_GetDadosEstagioIEL_Colunas({2},{3}, @d1, @d2) as f1
                              END", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, dataInicio, dataTermino);
        return getDataSet(comandoSQL);
    }

    public string getGraficoComparativoVagas(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        int i = 0;

        try
        {
            xmlAux.Append(string.Format(@"<set label=""Preenchidas"" value=""{0}"" />", dt.Rows[i]["QuantidadeVagasPreenchidas"].ToString()));

            xmlAux.Append(string.Format(@"<set label=""Abertas"" value=""{0}"" />", dt.Rows[i]["QuantidadeVagasNaoPreenchidas"].ToString()));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"8\" inThousandSeparator=\".\" thousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\" showZeroPies=\"0\" slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" formatNumberScale=\"0\" showLegend=\"1\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public DataSet getNumeroCadastrosEstagios(int codigoEntidade, int codigoUsuarioLogado, int ano, int mes)
    {
        comandoSQL = string.Format(@"
                              BEGIN

                                  SELECT f1.DataReferencia, f1.VariacaoQuantidadeAlunos, f1.VariacaoQuantidadeEmpresas, f1.VariacaoQuantidadeIES
                                    FROM {0}.{1}.f_sme_GetDadosEstagioIEL_GraficoCadastros({2},{3}, {4}, {5}) as f1
                              END", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, ano, mes);
        return getDataSet(comandoSQL);
    }

    public string getGraficoNumeroCadastros(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        int i = 0;

        try
        {
            string exportar = "";

            if (fonte > 12)
            {
                exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
            }
            else
            {
                exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
            }

            //configuração geral do gráfico
            xml.Append(string.Format(@"<chart caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""12""  scrollHeight=""12"" showLegend=""1""
                      BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1""
                      decimals=""0"" showShadow=""0"" {1} {2} showBorder=""0"" showSum=""0"" numberSuffix=""%"" slantLabels=""1"" labelDisplay=""ROTATE""
                      baseFontSize=""{3}""  inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" >"
                        , titulo
                        , usarGradiente + usarBordasArredondadas
                        , exportar
                        , fonte));

            xml.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0:MMM/yy}""/>", DateTime.Parse(dt.Rows[i]["DataReferencia"].ToString())));

            }

            xml.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(@"<dataset seriesName=""Alunos"">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""Alunos"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeAlunos"].ToString())));
            }

            xml.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(string.Format(@"<dataset seriesName=""Empresas"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""Empresas"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeEmpresas"].ToString()),
                    corSatisfatorio));
            }

            xml.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(string.Format(@"<dataset seriesName=""IES"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""IES"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeIES"].ToString()),
                   corCritico));
            }

            xml.Append("</dataset>");

        }
        catch
        {
            return "";
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

    public DataSet getNumeroDocumentosEstagios(int codigoEntidade, int codigoUsuarioLogado, int ano, int mes)
    {
        comandoSQL = string.Format(@"
                              BEGIN

                                  SELECT f1.DataReferencia, f1.VariacaoQuantidadeTCE, f1.VariacaoQuantidadeContratos, f1.VariacaoQuantidadeConvenios
                                    FROM {0}.{1}.f_sme_GetDadosEstagioIEL_GraficoDocumentos({2},{3}, {4}, {5}) as f1
                              END", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, ano, mes);
        return getDataSet(comandoSQL);
    }

    public string getGraficoNumeroDocumentos(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        int i = 0;

        try
        {
            string exportar = "";

            if (fonte > 12)
            {
                exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
            }
            else
            {
                exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
            }

            //configuração geral do gráfico
            xml.Append(string.Format(@"<chart caption=""{0}"" adjustDiv=""1"" numVisiblePlot=""12""  scrollHeight=""12"" showLegend=""1""
                      BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" showvalues=""0"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1""
                      decimals=""2"" showShadow=""0"" {1} {2} showBorder=""0"" showSum=""0"" numberSuffix=""%""  slantLabels=""1"" labelDisplay=""ROTATE""
                      baseFontSize=""{3}""  inThousandSeparator=""."" thousandSeparator=""."" inDecimalSeparator="","" decimalSeparator="","" >"
                        , titulo
                        , usarGradiente + usarBordasArredondadas
                        , exportar
                        , fonte));

            xml.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<category label=""{0:MMM/yy}""/>", DateTime.Parse(dt.Rows[i]["DataReferencia"].ToString())));

            }

            xml.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(@"<dataset seriesName=""TCE"">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""TCE"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeTCE"].ToString())));
            }

            xml.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(string.Format(@"<dataset seriesName=""Contratos"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""Contratos"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeContratos"].ToString()),
                    corSatisfatorio));
            }

            xml.Append("</dataset>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xml.Append(string.Format(@"<dataset seriesName=""Convênios"">"));

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""Convênios"" value=""{0}""/>", double.Parse(dt.Rows[i]["VariacaoQuantidadeConvenios"].ToString()),
                   corCritico));
            }

            xml.Append("</dataset>");

        }
        catch
        {
            return "";
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

    public DataSet getDadosEstagios(string codigoEntidade, int codigoUsuarioLogado, string data)
    {
        comandoSQL = string.Format(@"
                              BEGIN
                                 DECLARE @d1 smalldatetime, @d2 smalldatetime

                                    SET 	@d1 = CONVERT(SmallDateTime, '{4}', 103)
                                    SET 	@d2 = DATEADD(mm, -1, @d1)
                                    SELECT * FROM {0}.{1}.f_sme_GetDadosEstagioIEL_Colunas({2},{3}, @d2, @d1)
                              END", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, data);
        return getDataSet(comandoSQL);
    }

    public DataSet getVariacoesEstagios(string codigoEntidade, int codigoUsuarioLogado, string inicioAnterior, string terminoAnterior, string inicioAtual, string terminoAtual)
    {
        comandoSQL = string.Format(@"
                              BEGIN
                                    DECLARE @tblVar TABLE (SiglaUF Char(2), 
                                                             VarVagasOfertadas Decimal(5,0), 
                                                             VarVagasPreenchidas Decimal(5,0), 
                                                             VarVagasNaoPreenchidas Decimal(5,0), 
                                                             VarAlunos Decimal(5,0), 
                                                             VarEmpresas Decimal(5,0), 
                                                             VarIES Decimal(5,0), 
                                                             VarTCE Decimal(5,0), 
                                                             VarContratos Decimal(5,0), 
                                                             VarConvenios Decimal(5,0))

                                  DECLARE @inicioAnterior smalldatetime, @inicioAtual smalldatetime, @terminoAnterior smalldatetime, @terminoAtual smalldatetime
                                  
                                  SET 	@inicioAnterior = CONVERT(SmallDateTime, '{2}', 103)
                                  SET 	@inicioAtual = CONVERT(SmallDateTime, '{4}', 103)
                                  SET 	@terminoAnterior = CONVERT(SmallDateTime, '{3}', 103)
                                  SET 	@terminoAtual = CONVERT(SmallDateTime, '{5}', 103)


                                  INSERT INTO @tblVar                           
                                  SELECT Atual.SiglaUF, 
                                       CASE WHEN Sum(Anterior.QuantidadeVagasOfertadas) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeVagasOfertadas)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasOfertadas)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasOfertadas))*100 ELSE 0 END AS VarVagasOfertadas,
                                       CASE WHEN Sum(Anterior.QuantidadeVagasPreenchidas) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeVagasPreenchidas)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasPreenchidas)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasPreenchidas))*100 ELSE 0 END AS VarVagasPreenchidas,
                                       CASE WHEN Sum(Anterior.QuantidadeVagasNaoPreenchidas) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeVagasNaoPreenchidas)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasNaoPreenchidas)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeVagasNaoPreenchidas))*100 ELSE 0 END AS VarVagasNaoPreenchidas,
                                       CASE WHEN Sum(Anterior.QuantidadeAlunos) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeAlunos)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeAlunos)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeAlunos))*100 ELSE 0 END AS VarAlunos,
                                       CASE WHEN Sum(Anterior.QuantidadeEmpresas) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeEmpresas)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeEmpresas)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeEmpresas))*100 ELSE 0 END AS VarEmpresas,
                                       CASE WHEN Sum(Anterior.QuantidadeIES) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeIES)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeIES)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeIES))*100 ELSE 0 END AS VarIES,
                                       CASE WHEN Sum(Anterior.QuantidadeTCE) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeTCE)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeTCE)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeTCE))*100 ELSE 0 END AS VarTCE,
                                       CASE WHEN Sum(Anterior.QuantidadeContratos) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeContratos)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeContratos)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeContratos))*100 ELSE 0 END AS VarContratos,
                                       CASE WHEN Sum(Anterior.QuantidadeConvenios) <> 0 
                                            THEN Convert(Decimal(18,2),(Sum(Atual.QuantidadeConvenios)) - Convert(Decimal(18,2),Sum(Anterior.QuantidadeConvenios)))/Convert(Decimal(18,2),Sum(Anterior.QuantidadeConvenios))*100 ELSE 0 END AS VarConvenios
                                  FROM
	                                (SELECT * FROM {0}.{1}.f_sme_GetDadosEstagioIEL_Colunas 
		                                ({6},{7},@inicioAnterior,@terminoAnterior)) AS Anterior,
		                                 (SELECT * FROM {0}.{1}.f_sme_GetDadosEstagioIEL_Colunas 
		                                ({6},{7},@inicioAtual,@terminoAtual)) AS Atual
                                GROUP BY Atual.SiglaUF	

                                              
                                  SELECT MinVagasOfertadas.SiglaUF + '(' + CONVERT(Varchar,MinVagasOfertadas.VarVagasOfertadas) + '%)' AS MinOfertadas,
                                         MaxVagasOfertadas.SiglaUF + '(' + CONVERT(Varchar,MaxVagasOfertadas.VarVagasOfertadas) + '%)' AS MaxOfertadas,
		                                 MinVagasPreenchidas.SiglaUF + '(' + CONVERT(Varchar,MinVagasPreenchidas.VarVagasPreenchidas) + '%)' AS MinPreenchidas,
                                         MaxVagasPreenchidas.SiglaUF + '(' + CONVERT(Varchar,MaxVagasPreenchidas.VarVagasPreenchidas) + '%)' AS MaxPreenchidas,
		                                 MinVagasNaoPreenchidas.SiglaUF + '(' + CONVERT(Varchar,MinVagasNaoPreenchidas.VarVagasNaoPreenchidas) + '%)' AS MinNaoPreenchidas,
                                         MaxVagasNaoPreenchidas.SiglaUF + '(' + CONVERT(Varchar,MaxVagasNaoPreenchidas.VarVagasNaoPreenchidas) + '%)' AS MaxNaoPreenchidas,
		                                 MinAlunos.SiglaUF + '(' + CONVERT(Varchar,MinAlunos.VarAlunos) + '%)' AS MinAlunos,
                                         MaxAlunos.SiglaUF + '(' + CONVERT(Varchar,MaxAlunos.VarAlunos) + '%)' AS MaxAlunos,
		                                 MinEmpresas.SiglaUF + '(' + CONVERT(Varchar,MinEmpresas.VarEmpresas) + '%)' AS MinEmpresas,
                                         MaxEmpresas.SiglaUF + '(' + CONVERT(Varchar,MaxEmpresas.VarEmpresas) + '%)' AS MaxEmpresas,
		                                 MinIES.SiglaUF + '(' + CONVERT(Varchar,MinIES.VarIES) + '%)' AS MinIES,
                                         MaxIES.SiglaUF + '(' + CONVERT(Varchar,MaxIES.VarIES) + '%)' AS MaxIES,
		                                 MinTCE.SiglaUF + '(' + CONVERT(Varchar,MinTCE.VarTCE) + '%)' AS MinTCE,
                                         MaxTCE.SiglaUF + '(' + CONVERT(Varchar,MaxTCE.VarTCE) + '%)' AS MaxTCE,
		                                 MinContratos.SiglaUF + '(' + CONVERT(Varchar,MinContratos.VarContratos) + '%)' AS MinContratos,
                                         MaxContratos.SiglaUF + '(' + CONVERT(Varchar,MaxContratos.VarContratos) + '%)' AS MaxContratos,
		                                 MinConvenios.SiglaUF + '(' + CONVERT(Varchar,MinConvenios.VarConvenios) + '%)' AS MinConvenios,
                                         MaxConvenios.SiglaUF + '(' + CONVERT(Varchar,MaxConvenios.VarConvenios) + '%)' AS MaxConvenios
                                    FROM
                                  (SELECT Top 1 SiglaUF , VarVagasOfertadas FROM @tblVar ORDER BY VarVagasOfertadas) AS MinVagasOfertadas,  
                                  (SELECT Top 1 SiglaUF, VarVagasOfertadas FROM @tblVar ORDER BY VarVagasOfertadas DESC) AS MaxVagasOfertadas,
                                  (SELECT Top 1 SiglaUF , VarVagasPreenchidas FROM @tblVar ORDER BY VarVagasPreenchidas) AS MinVagasPreenchidas,  
                                  (SELECT Top 1 SiglaUF, VarVagasPreenchidas FROM @tblVar ORDER BY VarVagasPreenchidas DESC) AS MaxVagasPreenchidas,
                                  (SELECT Top 1 SiglaUF , VarVagasNaoPreenchidas FROM @tblVar ORDER BY VarVagasNaoPreenchidas) AS MinVagasNaoPreenchidas,  
                                  (SELECT Top 1 SiglaUF, VarVagasNaoPreenchidas FROM @tblVar ORDER BY VarVagasNaoPreenchidas DESC) AS MaxVagasNaoPreenchidas,
                                  (SELECT Top 1 SiglaUF , VarAlunos FROM @tblVar ORDER BY VarAlunos) AS MinAlunos,  
                                  (SELECT Top 1 SiglaUF, VarAlunos FROM @tblVar ORDER BY VarAlunos DESC) AS MaxAlunos,
                                  (SELECT Top 1 SiglaUF , VarEmpresas FROM @tblVar ORDER BY VarEmpresas) AS MinEmpresas,  
                                  (SELECT Top 1 SiglaUF, VarEmpresas FROM @tblVar ORDER BY VarEmpresas DESC) AS MaxEmpresas,
                                  (SELECT Top 1 SiglaUF , VarIES FROM @tblVar ORDER BY VarIES) AS MinIES,  
                                  (SELECT Top 1 SiglaUF, VarIES FROM @tblVar ORDER BY VarIES DESC) AS MaxIES,
                                  (SELECT Top 1 SiglaUF , VarTCE FROM @tblVar ORDER BY VarTCE) AS MinTCE,  
                                  (SELECT Top 1 SiglaUF, VarTCE FROM @tblVar ORDER BY VarTCE DESC) AS MaxTCE,
                                  (SELECT Top 1 SiglaUF , VarContratos FROM @tblVar ORDER BY VarContratos) AS MinContratos,  
                                  (SELECT Top 1 SiglaUF, VarContratos FROM @tblVar ORDER BY VarContratos DESC) AS MaxContratos,
                                  (SELECT Top 1 SiglaUF , VarConvenios FROM @tblVar ORDER BY VarConvenios) AS MinConvenios,  
                                  (SELECT Top 1 SiglaUF, VarConvenios FROM @tblVar ORDER BY VarConvenios DESC) AS MaxConvenios
                                  
                                END", bancodb, Ownerdb, inicioAnterior, terminoAnterior, inicioAtual, terminoAtual, codigoUsuarioLogado, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Visão Executiva
    // Em 25/09/2013 detectou-se que este método não é utilizado no sistema.
    public DataSet getListaObjetivosExecutivo(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN

                DECLARE @tbResumo TABLE(CodigoObjetoEstrategia int
                                 ,DescricaoObjetoEstrategia VarChar(250)
                                 ,CodigoResponsavelObjeto int
                                 ,NomeUsuario VarChar(60)
                                 ,Desempenho VarChar(10))

                INSERT INTO @tbResumo
                    SELECT CodigoObjetoEstrategia, DescricaoObjetoEstrategia, CodigoResponsavelObjeto, u.NomeUsuario
                         , {0}.{1}.f_GetCorObjetivo({2}, CodigoObjetoEstrategia, YEAR(GETDATE()), MONTH(GETDATE())) AS Desempenho
			          FROM {0}.{1}.ObjetoEstrategia oe LEFT JOIN
				           {0}.{1}.Usuario u ON u.CodigoUsuario = CodigoResponsavelObjeto
			         WHERE CodigoTipoObjetoEstrategia IN(SELECT CodigoTipoObjetoEstrategia 
                                                           FROM {0}.{1}.TipoObjetoEstrategia  toe
                                                          WHERE IniciaisTipoObjeto = 'OBJ'
                                                            AND oe.DataExclusao IS NULL)
			           AND oe.DataExclusao IS NULL
			         ORDER BY OrdemObjeto

                SELECT * FROM @tbResumo
                 WHERE 1 = 1               
                   {3}
            END
            ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMensagensNovasExecutivo(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @CodigoUsuario int,
			            @CodigoEntidade int,
			            @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
            			
	            SET @CodigoUsuario = {3}
	            SET @CodigoEntidade = {2}

	            SELECT m.CodigoMensagem, m.Mensagem, m.DataInclusao, m.DataLimiteResposta, u.NomeUsuario
					 , CASE WHEN m.IndicaRespostaNecessaria = 'S' THEN 'Sim' ELSE 'Não' END AS RespostaNecessaria
                     , cm.Assunto
	              FROM {0}.{1}.Mensagem m INNER JOIN
		               {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
		               {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
	                   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	             WHERE md.CodigoDestinatario = @CodigoUsuario
	               AND md.DataLeitura IS NULL
	               AND m.CodigoEntidade = @CodigoEntidade    
                   AND cm.IndicaTipoMensagem = 'E'  
                   AND cm.[CodigoUsuario]	= md.[CodigoDestinatario]
                   {4}       	
            END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMensagensAbertasExecutivo(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN 
                DECLARE @CodigoUsuario int,
			            @CodigoEntidade int,
			            @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
                            			
            SET @CodigoUsuario = {3}
            SET @CodigoEntidade = {2}

            SELECT DISTINCT m.CodigoMensagem, CONVERT(Varchar(1000),m.Mensagem,1) as Mensagem, m.DataInclusao, m.DataLimiteResposta, null AS UsuarioResposta
	         , m.DataResposta, m.Resposta
	         , CASE WHEN CONVERT(VarChar, m.DataLimiteResposta, 102) < CONVERT(VarChar, GetDate(), 102) THEN 'S' ELSE 'N' END AS Atrasada
	         , cm.Assunto
              FROM {0}.{1}.Mensagem m INNER JOIN
	           {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
             WHERE m.CodigoUsuarioInclusao = @CodigoUsuario
               AND m.DataResposta IS NULL
               AND m.IndicaRespostaNecessaria = 'S'
               AND m.CodigoEntidade = @CodigoEntidade
               AND cm.IndicaTipoMensagem = 'E'
               {4}
        END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMensagensRespondidasExecutivo(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @CodigoUsuario int,
			            @CodigoEntidade int,
			            @codigoTipoAssociacao Int

                SELECT @codigoTipoAssociacao =  CodigoTipoAssociacao 
                  FROM {0}.{1}.TipoAssociacao 
                 WHERE IniciaisTipoAssociacao = 'MG'
            			
	            SET @CodigoUsuario = {3}
	            SET @CodigoEntidade = {2}

	            SELECT m.CodigoMensagem, m.Mensagem, m.DataInclusao, m.DataLimiteResposta, u.NomeUsuario AS UsuarioResposta
					 , m.DataResposta, m.Resposta, cm.Assunto
	              FROM {0}.{1}.Mensagem m INNER JOIN
                       {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioResposta INNER JOIN
					   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem
																 AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	             WHERE m.CodigoUsuarioInclusao = @CodigoUsuario
	               AND m.DataResposta IS NOT NULL
	               AND m.CodigoEntidade = @CodigoEntidade
                   AND cm.IndicaTipoMensagem = 'E'
                   {4}      	
            END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getNumeroMensagensExecutivo(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
	            DECLARE @MensagensNovas int,
			            @MensagensNaoRespondidas int,
			            @Reunioes int,
                        @Fluxos int,
			            @CodigoUsuario int,
			            @CodigoEntidade int,
                        @RC int,
                        @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
            			
	            SET @CodigoUsuario = {3}
	            SET @CodigoEntidade = {2}

	            SELECT @MensagensNovas = COUNT(1) 
	              FROM {0}.{1}.Mensagem m INNER JOIN
		               {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
	                   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	             WHERE md.CodigoDestinatario = @CodigoUsuario
	               AND md.DataLeitura IS NULL
	               AND m.CodigoEntidade = @CodigoEntidade
                   AND cm.IndicaTipoMensagem = 'E'  
                   AND cm.[CodigoUsuario]	= md.[CodigoDestinatario]
		           AND dbo.f_verificaObjetoMonitoravel(m.[CodigoMensagem], NULL, 'MG', 0, m.[CodigoEntidade], m.[CodigoEntidade], NULL, 'EN', 0) = 1
            	   
	            SELECT @MensagensNaoRespondidas = COUNT(1) 
	              FROM {0}.{1}.Mensagem m INNER JOIN
	                   {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	             WHERE m.CodigoUsuarioInclusao = @CodigoUsuario
	               AND m.DataResposta IS NULL
	               AND m.IndicaRespostaNecessaria = 'S'
	               AND m.CodigoEntidade = @CodigoEntidade
                   AND cm.IndicaTipoMensagem = 'E'
		           AND dbo.f_verificaObjetoMonitoravel(m.[CodigoMensagem], NULL, 'MG', 0, m.[CodigoEntidade], m.[CodigoEntidade], NULL, 'EN', 0) = 1
            	   
	             SELECT @Reunioes = COUNT(DISTINCT pe.CodigoEvento)
                    FROM {0}.{1}.ParticipanteEvento AS pe INNER JOIN
                         {0}.{1}.Evento AS e ON (e.CodigoEvento = pe.CodigoEvento
                                     AND e.CodigoEntidade = @CodigoEntidade
                                     AND ((Convert(Varchar,e.InicioPrevisto,112) >= Convert(Varchar,GETDATE(),112) AND Convert(Varchar,e.InicioPrevisto,112) < Convert(Varchar,GETDATE()+7,112))
                                     AND (Convert(Varchar,e.TerminoPrevisto,112) >= Convert(Varchar,GETDATE(),112) AND Convert(Varchar,e.TerminoPrevisto,112) < Convert(Varchar,GETDATE()+7,112))))
                   WHERE pe.CodigoParticipante = @CodigoUsuario
                      OR e.CodigoResponsavelEvento = @CodigoUsuario
                     -- AND pe.DataConfirmacaoParticipante IS NOT NULL -- Esta cláusula será incluída somente quando a opção para confirmar participação estiver desenvolvida.
                    
                
                EXECUTE @RC = {0}.{1}.[p_wf_GetMensagens] 
                   @CodigoUsuario
                  ,@CodigoEntidade
                  ,'QTD_E'
                  ,NULL
                  ,@Fluxos output  
            	   
	             SELECT ISNULL(@MensagensNovas, 0) AS MensagensNovas, 
			            ISNULL(@MensagensNaoRespondidas, 0) AS MensagensNaoRespondidas,
			            ISNULL(@Reunioes, 0) AS Reunioes,
                        ISNULL(@Fluxos, 0) AS Fluxos
            	
            END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosProjetoExecutivo(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN 
              DECLARE @CodigoProjetoParam      Int,
                      @Riscos                  Int,
                      @Questoes                      Int,
                      @IDP                           Decimal(25,4),
                      @IDC                           Decimal(25,4)
                      
                SET @CodigoProjetoParam  = {2}
                
                SELECT @IDP = {0}.{1}.f_GetIDPAtual(@CodigoProjetoParam)
                SELECT @IDC = {0}.{1}.f_GetIDCAtual(@CodigoProjetoParam)
                
              
             /* Obtém a lista de riscos ativos */
            SELECT @Riscos = Count(1)
               FROM {0}.{1}.RiscoQuestao AS rq 
              WHERE rq.IndicaRiscoQuestao = 'R'
                AND rq.CodigoStatusRiscoQuestao = 'RA'
                AND rq.DataExclusao IS NULL
                AND rq.DataPublicacao IS NOT NULL
                AND rq.CodigoProjeto = @CodigoProjetoParam

                
              SELECT @Questoes = Count(1)
               FROM {0}.{1}.RiscoQuestao AS rq 
              WHERE rq.IndicaRiscoQuestao = 'Q'
                AND rq.CodigoStatusRiscoQuestao = 'QA'
                AND rq.DataExclusao IS NULL
                AND rq.DataPublicacao IS NOT NULL
                AND rq.CodigoProjeto = @CodigoProjetoParam
              
               
               SELECT ISNULL(@IDP-1, 0) AS DesvioFisico,
                      ISNULL(1-@IDC, 0) AS DesvioFinanceiro,
                      ISNULL(@Riscos, 0) AS Riscos, ISNULL(@Questoes, 0) AS Questoes
                  
            END

            ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoMetas(DataTable dt, string titulo, int fonte, bool linkLista)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {
            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=N&Azul=S&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Superada"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=S&Amarelo=N&Vermelho=N&Azul=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Atingida"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=S&Vermelho=N&Azul=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Abaixo(Alerta)"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=S&Azul=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Muito Abaixo"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=N&Azul=N&Branco=S&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Desatualizada"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString(),
                                                                                                           "8A8A8A",
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"1\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"1\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
            " showValues=\"1\" use3Dlighting=\"0\" legendShadow=\"0\" legendBorderAlpha=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Roboto\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Roboto\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
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

    public string getGraficoDesempenhoProjetosExecutivo(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarLegenda)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutivaProjetos/Projetos/listaProjetos.aspx?Fechados=S&Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Satisfatório"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutivaProjetos/Projetos/listaProjetos.aspx?Fechados=S&Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutivaProjetos/Projetos/listaProjetos.aspx?Fechados=S&Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Branco=N&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Crítico"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (mostraLaranja == "S")
            {
                if (linkLista)
                    link = @"link=""F-_top-../../_VisaoExecutivaProjetos/Projetos/listaProjetos.aspx?Fechados=S&Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Branco=N&NivelNavegacao=1"" ";

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString()
                                                                                                              , "FF6600"
                                                                                                              , link));
            }

            if (linkLista)
                link = @"link=""F-_top-../../_VisaoExecutivaProjetos/Projetos/listaProjetos.aspx?Fechados=S&Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Branco=S&NivelNavegacao=1"" ";

            xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString(),
                                                                                                           "F3F3F3",
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"" + mostrarLegenda + "\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"5\" " +
            " showValues=\"1\" use3Dlighting=\"0\" legendShadow=\"0\" legendBorderAlpha=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Roboto\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Roboto\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
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

    #endregion

    #region Painel Analista

    public DataSet getAtualizacoesPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where, char tipoDado3, string whereCronograma)
    {
        string comandoSQLTipoDado3 = "";
        if (tipoDado3 == 'P')
        {
            comandoSQLTipoDado3 = string.Format(@"
                     SELECT 3,
                     'Parcelas Contratos',
                     IsNull(SUM(1),0),
                     IsNull(SUM(CASE WHEN DATEADD(DD,1,pc.[DataVencimento])<GETDATE() THEN 1 ELSE 0 END),0)
                FROM {0}.{1}.ParcelaContrato AS pc INNER JOIN
                     {0}.{1}.Contrato AS c ON (c.CodigoContrato = pc.CodigoContrato
                                   AND c.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                                   AND c.StatusContrato = 'A'
                                   AND c.CodigoEntidade = @CodigoEntidadeParam) LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                            {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = c.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) INNER JOIN
                     {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = c.CodigoUnidadeNegocio
                                          AND un.CodigoEntidade = @CodigoEntidadeParam
                                          AND un.DataExclusao IS NULL
                                          AND un.[IndicaUnidadeNegocioAtiva] = 'S')
               WHERE pc.DataPagamento IS NULL  AND pc.[DataExclusao] IS NULL 
                AND DATEDIFF(DD, GETDATE(), pc.[DataVencimento])<=@diasParcelasVencendo ", bancodb, Ownerdb);
        }
        else if (tipoDado3 == 'L')
        {
            comandoSQLTipoDado3 = string.Format(@"
            SELECT 
            3
            , 'Lançamentos Financeiros'
            , COUNT(1)
            , ISNULL(SUM(CASE WHEN DATEADD(DD,1,f.[DataVencimento])<GETDATE() THEN 1 ELSE 0 END),0)
            FROM [dbo].f_gestconv_GetLancamentosFinanceiros(@CodigoEntidadeParam, NULL, @CodigoUsuarioParam, 'P', 'S') AS f");
        }


        string comandoSQL = string.Format(@"
            BEGIN
              DECLARE @CodigoUsuarioParam Int,
                      @CodigoEntidadeParam Int,
                      @ResourceUID VarChar(64), 
					  @DataInicioDia	SmallDateTime
											
              SET @DataInicioDia = CONVERT(SmallDateTime, CONVERT(Varchar(10), GETDATE(), 103), 103)      
              SET @CodigoUsuarioParam = {3}
              SET @CodigoEntidadeParam = {2}

              SELECT @ResourceUID = CodigoRecursoMSProject 
                FROM {0}.{1}.vi_RecursoCorporativo 
               WHERE CodigoRecurso = @CodigoUsuarioParam
                 AND CodigoEntidade = @CodigoEntidadeParam

              DECLARE @diasParcelasVencendo Int 
              SELECT @diasParcelasVencendo = CAST(pcs.[Valor] AS Int) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasParcelasVencendo'
              IF (@diasParcelasVencendo IS NULL)
                SET @diasParcelasVencendo = 10

                DECLARE @TotalTarefasNaoConcluidas INT
                DECLARE @TotalTarefasAtrasadas INT
                DECLARE @TotalTarefasConcluidas INT
                BEGIN
                    DECLARE
                        @DataInicioParam                DATETIME,
                        @DataTerminoParam               DATETIME,
                        @DataTarefasParam               DATETIME,
                        @DataToDoParam                  DATETIME,
                        @CodigoRecursoParam             INT,
                        @CodigoProjeto                  INT,
                        @CodigoEntidade                 INT,
                        @TipoAtualizacao                CHAR(2),
                        @Status                         VARCHAR(20),
                        @PesquisaExibirCardsAtrasados   BIT,
                        @PesquisaDataInicio             VARCHAR(10),
                        @PesquisaDataFim                VARCHAR(10),
                        @PesquisaTexto                  VARCHAR(100),
                        @Ordenacao                      VARCHAR(20),
                        @Pagina                         INT;

                    SET @DataInicioParam  = null;
                    SET @DataTerminoParam = null;
                    SET @DataTarefasParam = null;
                    SET @DataToDoParam = null;
                    SET @CodigoRecursoParam = @CodigoUsuarioParam;
                    SET @CodigoProjeto = -1;
                    SET @TipoAtualizacao = 'TD';
                    SET @CodigoEntidade = @CodigoEntidadeParam;
                    SET @Status = null;
                    SET @PesquisaExibirCardsAtrasados = 0;
                    SET @PesquisaDataInicio = null;
                    SET @PesquisaDataFim = null;
                    SET @PesquisaTexto = '';
                    SET @Ordenacao = 'prioridade';
                    SET @Pagina = 0;

                      EXEC dbo.p_TarefasTimeSheetRecursoKanbanPesquisaOrdenacao @DataInicioParam
                                                , @DataTerminoParam
                                                , @DataTarefasParam
                                                , @DataToDoParam
                                                , @CodigoRecursoParam
                                                , @CodigoProjeto
                                                , @TipoAtualizacao
                                                , @CodigoEntidade
                                                , @Status
                                                , @PesquisaExibirCardsAtrasados
                                                , @PesquisaDataInicio
                                                , @PesquisaDataFim
                                                , @PesquisaTexto
                                                , @Ordenacao
                                                , @Pagina = @Pagina
								                , @TotalTarefasNaoConcluidas = @TotalTarefasNaoConcluidas OUTPUT
								                , @TotalTarefasAtrasadas = @TotalTarefasAtrasadas OUTPUT
								                , @TotalTarefasConcluidas = @TotalTarefasConcluidas OUTPUT;

                END

              SELECT 1 AS Codigo,
                     'Cronograma' AS Descricao,
                     @TotalTarefasNaoConcluidas AS Total,
                     @TotalTarefasAtrasadas AS Atrasados
              /*
              SELECT 1 AS Codigo,
                     'Cronograma' AS Descricao,
                     COUNT(1) AS Total,
                     ISNULL(Sum(CASE WHEN (IndicaTarefaAtrasada = 'S') THEN 1 ELSE 0 END), 0) AS Atrasados
                FROM {0}.{1}.f_getTarefasUsuario(@CodigoUsuarioParam, @CodigoEntidadeParam)                   
               WHERE IndicaToDoList = 'N'
                 --AND dbo.f_verificaObjetoMonitoravel(CodigoProjeto, dbo.f_GetCodigoTipoAssociacao('PR'), NULL, 0, @CodigoEntidadeParam, @CodigoEntidadeParam, NULL, 'EN', 0) = 1
                 {5}
              */
            UNION                           
              SELECT 2,
				    'Indicadores',
                    ( SELECT COUNT(1) 
						FROM 
						    {0}.{1}.[IndicadorUnidade]			AS [iu]
										
							INNER JOIN {0}.{1}.[Indicador]		AS [i]		ON 
							(		i.[CodigoIndicador]	= iu.[CodigoIndicador]
							    AND i.[DataExclusao]	IS NULL )
												
							INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]		ON 
							(		un.[CodigoUnidadeNegocio]	= iu.[CodigoUnidadeNegocio]
							    AND un.[CodigoEntidade]			= @CodigoEntidadeParam
								AND un.[DataExclusao]			IS NULL
								AND un.[IndicaUnidadeNegocioAtiva] = 'S' )
						WHERE
							    iu.[DataExclusao]			IS NULL
						    AND iu.[CodigoResponsavelAtualizacaoIndicador] = @CodigoUsuarioParam 
                            AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,'01/01/1900') AND IsNull(i.DataTerminoValidadeMeta,'31/12/2999')) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
							AND {0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu.[CodigoUnidadeNegocio],iu.[CodigoIndicador]) < @DataInicioDia+10 ), 
                    ( SELECT COUNT(1) 
					    FROM 
						    {0}.{1}.[IndicadorUnidade]			AS [iu]
										
							INNER JOIN {0}.{1}.[Indicador]		AS [i]		ON 
							(		i.[CodigoIndicador]	= iu.[CodigoIndicador]
							    AND i.[DataExclusao]	IS NULL )
												
							INNER JOIN {0}.{1}.[UnidadeNegocio]	AS [un]		ON 
								(		un.[CodigoUnidadeNegocio]	= iu.[CodigoUnidadeNegocio]
								    AND un.[CodigoEntidade]			= @CodigoEntidadeParam
									AND un.[DataExclusao]			IS NULL
									AND un.[IndicaUnidadeNegocioAtiva] = 'S' )
						WHERE
							    iu.[DataExclusao]			IS NULL
							AND iu.[CodigoResponsavelAtualizacaoIndicador] = @CodigoUsuarioParam 
                            AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,'01/01/1900') AND IsNull(i.DataTerminoValidadeMeta,'31/12/2999')) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
							AND {0}.{1}.f_GetDataVencimentoAtualizacaoIndicador(iu.[CodigoUnidadeNegocio],iu.[CodigoIndicador]) < @DataInicioDia )
            UNION                   
               --comandoSQLTipoDado3 Task 892: [FIEB] - Alterar a função C# cDados.getAtualizacoesPainelAnalista para corresponder ao novo frame Atualizações
             {6}  
            UNION
              SELECT 4,
                     'To Do List',
                     0 AS Total,
                     0 AS Atrasados
            /*
              SELECT 4,
                     'To Do List',
                     COUNT(1) AS Total,
                     ISNULL(Sum(CASE WHEN (IndicaTarefaAtrasada = 'S') THEN 1 ELSE 0 END), 0) AS Atrasados
                FROM {0}.{1}.f_getTarefasUsuario(@CodigoUsuarioParam, @CodigoEntidadeParam)                   
               WHERE IndicaToDoList = 'S'
                 --AND Convert(Varchar, TerminoPrevisto,102) <= Convert(Varchar,GETDATE()+10,102) 
                 {5}
            */
            END

            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where, whereCronograma, comandoSQLTipoDado3);
        var ds = getDataSet(comandoSQL);
        ds.Tables.RemoveAt(0);
        return ds;
    }

    public DataSet getFiscalizacoesPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
              DECLARE @CodigoUsuarioParam Int,
                      @CodigoEntidadeParam Int
                            
              SET @CodigoUsuarioParam = {3}
              SET @CodigoEntidadeParam = {2}    

             DECLARE @diasAtualizacaoCronograma Int 
             
             SELECT @diasAtualizacaoCronograma = CAST(pcs.[Valor] AS Int) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasAtualizacaoCronograma'
              
             IF (@diasAtualizacaoCronograma IS NULL)
                SET @diasAtualizacaoCronograma = 10   

             DECLARE @diasAtualizacaoFotos Int 
             
             SELECT @diasAtualizacaoFotos = CAST(pcs.[Valor] AS Int) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasAtualizacaoFotos'
              
             IF (@diasAtualizacaoFotos IS NULL)
                SET @diasAtualizacaoFotos = 30   

             DECLARE @diasComentariosFiscalizacao Int 
             
             SELECT @diasComentariosFiscalizacao = CAST(pcs.[Valor] AS Int) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasComentariosFiscalizacao'
              
             IF (@diasComentariosFiscalizacao IS NULL)
                SET @diasComentariosFiscalizacao = 30       
                    
              SELECT 1 AS Codigo,
                     'Atualizacao Cronograma' AS Descricao,
                     IsNull(SUM(1),0) AS Total
                FROM {0}.{1}.f_obr_GetDetalhesObras(@CodigoUsuarioParam, @CodigoEntidadeParam) AS f
               WHERE CodigoStatus = 3
                 AND (DATEDIFF(DD, f.[UltimaAtualizacaoCronograma], GETDATE()) >= @diasAtualizacaoCronograma
                  OR f.[UltimaAtualizacaoCronograma] IS NULL)
            UNION                           
              SELECT 2,
                     'Atualizacao Fotos',
                     IsNull(SUM(1),0)
                FROM {0}.{1}.f_obr_GetDetalhesObras(@CodigoUsuarioParam, @CodigoEntidadeParam) AS f
               WHERE CodigoStatus = 3
                 AND (DATEDIFF(DD, f.[UltimaAtualizacaoFoto], GETDATE()) >= @diasAtualizacaoFotos  
                  OR f.[UltimaAtualizacaoFoto] IS NULL)
            UNION                   
              SELECT 3,
                     'Comentários Fiscalização',
                     IsNull(SUM(1),0)
                FROM {0}.{1}.f_obr_GetDetalhesObras(@CodigoUsuarioParam, @CodigoEntidadeParam) AS f
               WHERE CodigoStatus = 3
                 AND (DATEDIFF(DD, f.[UltimaAnaliseFiscalizacao], GETDATE()) >= @diasComentariosFiscalizacao 
                  OR f.[UltimaAnaliseFiscalizacao] IS NULL)
             
            END

            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getGestaoPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, int quantidadeDiasAlertaContratos, string where)
    {
        string comandoSQL = string.Format(@"
           BEGIN
                DECLARE @CodigoUsuarioParam Int,
                        @CodigoEntidadeParam Int
                        
                SET @CodigoUsuarioParam = {3}
                SET @CodigoEntidadeParam = {2}

                 SELECT 1 AS Codigo,
                                           'Riscos' AS Descricao,
                                           IsNull(SUM(1),0) AS Total,
                                           IsNull(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END),0) AS Atrasados
                                    FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN
                                         {0}.{1}.Projeto as PRJ on (PRJ.CodigoProjeto = rq.CodigoProjeto
                                                                    AND PRJ.CodigoStatusProjeto = 3)
                              WHERE rq.CodigoStatusRiscoQuestao = 'RA'
                                 AND rq.DataExclusao IS NULL
                                 AND rq.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                                 AND rq.CodigoEntidade = @CodigoEntidadeParam
                                 AND (dbo.f_verificaObjetoValido(RQ.CodigoRiscoQuestao, NULL, 'RQ', 0, RQ.CodigoEntidade, RQ.CodigoEntidade, NULL, 'EN', 0) = 1)
                                 AND RQ.IndicaRiscoQuestao = 'R' 
                 UNION                   
                  SELECT 2 AS Codigo,
                                           'Questões' AS Descricao,
                                           IsNull(SUM(1),0) AS Total,
                                           IsNull(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END),0) AS Atrasados
                                    FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN
                                         {0}.{1}.Projeto as PRJ on (PRJ.CodigoProjeto = rq.CodigoProjeto
                                                                    AND PRJ.CodigoStatusProjeto = 3)
                              WHERE rq.CodigoStatusRiscoQuestao = 'QA'
                                 AND rq.DataExclusao IS NULL
                                 AND rq.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                                 AND rq.CodigoEntidade = @CodigoEntidadeParam
                                 AND (dbo.f_verificaObjetoValido(RQ.CodigoRiscoQuestao, NULL, 'RQ', 0, RQ.CodigoEntidade, RQ.CodigoEntidade, NULL, 'EN', 0) = 1)
                                 AND RQ.IndicaRiscoQuestao = 'Q' 
                 UNION                   
                  SELECT 3,
                         'Contratos',
                         IsNull(SUM(CASE WHEN Convert(Varchar,c.DataTermino,102) >= Convert(Varchar,GETDATE(),102) THEN 1 ELSE 0 END),0),
                         IsNull(SUM(CASE WHEN CONVERT(VarChar, c.DataTermino, 102) < CONVERT(VarChar, GetDate(), 102) THEN 1 ELSE 0 END),0) AS Atrasados
                     FROM {0}.{1}.Contrato AS c LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                            {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = c.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) INNER JOIN
                         {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = c.CodigoUnidadeNegocio
                                                      AND un.CodigoEntidade = @CodigoEntidadeParam
                                                      AND un.DataExclusao IS NULL)              
                   WHERE Convert(Varchar,c.DataTermino,102) <= Convert(Varchar,GETDATE()+ {5},102) 
                     AND c.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                     AND c.StatusContrato = 'A' 
                     AND c.CodigoEntidade = @CodigoEntidadeParam
              END

            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where, quantidadeDiasAlertaContratos);
        return getDataSet(comandoSQL);
    }

    public DataSet getAprovacoesPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where)
    {

        string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @RC int

                DECLARE @CodigoUsuario int 
                DECLARE @CodigoEntidade int 
                DECLARE @TipoRetorno char(5)
                DECLARE @QtdFluxos int 
               

                -- TODO: Defina valores de parâmetros aqui.
                SET @CodigoUsuario = {3}
                SET @CodigoEntidade  = {2} 
                SET @TipoRetorno  = 'QTD_A'
              

                EXECUTE @RC = {0}.{1}.[p_wf_GetMensagens] 
                   @CodigoUsuario
                  ,@CodigoEntidade
                  ,@TipoRetorno
                  ,NULL
                  ,@QtdFluxos output
            END

        ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getComunicacaoPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int,
			              @codigoTipoAssociacao Int

                  SELECT @codigoTipoAssociacao =  CodigoTipoAssociacao 
                    FROM {0}.{1}.TipoAssociacao 
                   WHERE IniciaisTipoAssociacao = 'MG'
                          
                  SET @CodigoUsuarioParam = {3}
                  SET @CodigoEntidadeParam = {2}
                  
                  SELECT 1 AS Codigo,
					     'Compromissos' AS Descricao,
                         IsNull(SUM(1),0) AS Total,
                         IsNull(SUM(CASE WHEN Convert(Varchar,e.InicioPrevisto,112) < Convert(Varchar,GETDATE()+10,112) THEN 1 ELSE 0 END),0) AS Atrasados
                    FROM(					
                        SELECT DataInicio AS InicioPrevisto
                         FROM {0}.{1}.CompromissoUsuario
                        WHERE CodigoUsuario = @CodigoUsuarioParam
                          AND CodigoEntidade = @CodigoEntidadeParam
                		  UNION
						SELECT e.InicioPrevisto
						 FROM {0}.{1}.Evento as e
						WHERE (e.CodigoEvento IN (SELECT pe.CodigoEvento FROM {0}.{1}.ParticipanteEvento pe WHERE pe.CodigoParticipante = @CodigoUsuarioParam)
									OR e.CodigoResponsavelEvento = @CodigoUsuarioParam)
						  AND CodigoEntidade = @CodigoEntidadeParam
                          AND e.TerminoReal IS NULL) AS e                                     
                                    WHERE (Convert(Varchar,e.InicioPrevisto,112) >= Convert(Varchar,GETDATE(),112))
                   UNION                   
                   SELECT 2,
                          'MensagensRecebidas',
                          IsNull(SUM(1),0),
                          IsNull(SUM(CASE WHEN m.IndicaRespostaNecessaria = 'S' THEN 1 ELSE 0 END),0)
                     FROM {0}.{1}.Mensagem m INNER JOIN
                          {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
                          {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
                          {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
                    WHERE md.CodigoDestinatario = @CodigoUsuarioParam
                      AND md.DataLeitura IS NULL
                      AND m.CodigoEntidade = @CodigoEntidadeParam    
                      AND cm.IndicaTipoMensagem = 'E'  
                      AND cm.[CodigoUsuario] = md.[CodigoDestinatario]
		              AND {0}.{1}.f_verificaObjetoMonitoravel(m.[CodigoMensagem], NULL, 'MG', 0, m.[CodigoEntidade], m.[CodigoEntidade], NULL, 'EN', 0) = 1
                   UNION                   
                   SELECT 3,
						  'Respostas',
						  IsNull(SUM(1),0),
						  IsNull(SUM(CASE WHEN Convert(Varchar,m.DataLimiteResposta,112) < Convert(Varchar,GETDATE(),112) THEN 1 ELSE 0 END),0)
                    FROM {0}.{1}.Mensagem m
	             WHERE m.CodigoUsuarioInclusao = @CodigoUsuarioParam
	               AND m.DataResposta IS NULL
	               AND m.IndicaRespostaNecessaria = 'S'
	               AND m.CodigoEntidade = @CodigoEntidadeParam
                                             
                END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getPendenciasPainelRecursos(int codigoEntidade, int codigoUsuarioLogado, string where, string whereCronograma)
    {
        string comandoSQL = string.Format(@"
            BEGIN
              DECLARE @CodigoUsuarioParam Int,
                      @CodigoEntidadeParam Int,
                      @ResourceUID VarChar(64), 
					  @DataInicioDia	SmallDateTime,
			          @codigoTipoAssociacao Int

                  SELECT @codigoTipoAssociacao =  CodigoTipoAssociacao 
                    FROM {0}.{1}.TipoAssociacao 
                   WHERE IniciaisTipoAssociacao = 'MG'
											
              SET @DataInicioDia = CONVERT(SmallDateTime, CONVERT(Varchar(10), GETDATE(), 103), 103)      
              SET @CodigoUsuarioParam = {3}
              SET @CodigoEntidadeParam = {2}

              SELECT @ResourceUID = CodigoRecursoMSProject 
                FROM {0}.{1}.vi_RecursoCorporativo 
               WHERE CodigoRecurso = @CodigoUsuarioParam
                 AND CodigoEntidade = @CodigoEntidadeParam             
                    
              SELECT 1 AS Codigo,
                     'Atualização de Tarefas' AS Descricao,
                     COUNT(1) AS Total,
                     ISNULL(Sum(CASE WHEN (IndicaTarefaAtrasada = 'S') THEN 1 ELSE 0 END), 0) AS Atrasados
                FROM {0}.{1}.f_getTarefasUsuario(@CodigoUsuarioParam, @CodigoEntidadeParam)                   
               WHERE 1 = 1
                 {5}
            UNION                           
              SELECT 2 AS Codigo,
                     'Aprovação de Tarefas' AS Descricao,
                     COUNT(1) AS Total,
                     NULL AS Atrasados
                    FROM dbo.AtribuicaoRecursoTarefa AS art INNER JOIN
                         dbo.CronogramaProjeto AS cp ON (cp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                               AND cp.CodigoEntidade = @CodigoEntidadeParam) INNER JOIN
                         dbo.TarefaCronogramaProjeto AS tcp ON (tcp.CodigoCronogramaProjeto = art.CodigoCronogramaProjeto
                                                      AND tcp.CodigoTarefa = art.CodigoTarefa
                                                      AND tcp.DataExclusao IS NULL) INNER JOIN
                         dbo.Projeto AS p on (p.CodigoProjeto = cp.CodigoProjeto
                                                  AND p.DataExclusao IS NULL) -- Só projetos não excluídos                        
                   WHERE art.StatusAprovacao IN ('PA','EA','ER')    --> Pendente de Aprovação, Em Aprovação ou Em Reprovação  
                     AND EXISTS (SELECT 1
                                   FROM dbo.f_GetAprovadorAtribuicao(art.CodigoAtribuicao) AS f
                                  WHERE f.CodigoAprovador = @CodigoUsuarioParam)
            UNION                   
              SELECT 3 AS Codigo,
                     'Riscos' AS Descricao,
                     IsNull(SUM(1),0) AS Total,
                     IsNull(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END),0) AS Atrasados
                FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN
                     {0}.{1}.Projeto as PRJ on (PRJ.CodigoProjeto = rq.CodigoProjeto
                                            AND PRJ.CodigoStatusProjeto = 3)
               WHERE rq.CodigoStatusRiscoQuestao = 'RA'
                 AND rq.DataExclusao IS NULL
                 AND rq.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                 AND rq.CodigoEntidade = @CodigoEntidadeParam
                 AND RQ.DataExclusao IS NULL
                 AND RQ.IndicaRiscoQuestao = 'R' 
            UNION                   
              SELECT 4 AS Codigo,
                     'Questões' AS Descricao,
                     IsNull(SUM(1),0) AS Total,
                     IsNull(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END),0) AS Atrasados
               FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN
                    {0}.{1}.Projeto as PRJ on (PRJ.CodigoProjeto = rq.CodigoProjeto
                                           AND PRJ.CodigoStatusProjeto = 3)
              WHERE rq.CodigoStatusRiscoQuestao = 'QA'
                AND rq.DataExclusao IS NULL
                AND rq.CodigoUsuarioResponsavel = @CodigoUsuarioParam
                AND rq.CodigoEntidade = @CodigoEntidadeParam
                AND RQ.DataExclusao IS NULL
                AND RQ.IndicaRiscoQuestao = 'Q' 
            UNION           
              SELECT 5,
					      'MensagensRecebidas',
						  IsNull(SUM(1),0),
						  IsNull(SUM(CASE WHEN m.IndicaRespostaNecessaria = 'S' THEN 1 ELSE 0 END),0)
                    FROM {0}.{1}.Mensagem m INNER JOIN
		                 {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
		                 {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
	                     {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	               WHERE md.CodigoDestinatario = @CodigoUsuarioParam
	                 AND md.DataLeitura IS NULL
	                 AND m.CodigoEntidade = @CodigoEntidadeParam    
                     AND cm.IndicaTipoMensagem = 'E'  
                     AND cm.[CodigoUsuario]	= md.[CodigoDestinatario]
            END

            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where, whereCronograma);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoMetasAnalista(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"BEGIN 
	            DECLARE @CodigoEntidadLogada int,
                        @AnoAtual SmallInt,
                        @MesAtual SmallInt,
                        @CodigoUsuario int

	            DECLARE @tbCores Table(Cor Varchar(10))
                DECLARE @tbValores Table(Satisfatorio INT
                                        ,Atencao INT
                                        ,Excelente INT
                                        ,Laranja INT
                                        ,Sem_Acompanhamento INT
                                        ,Critico INT)
            	
	            SET @CodigoEntidadLogada = {2}
	            SET @AnoAtual = Year(GetDate())
                SET @MesAtual = Month(GetDate())
                SET @CodigoUsuario = {3}
	
	            INSERT INTO @tbCores
		            SELECT {0}.{1}.f_GetUltimoDesempenhoIndicador(iu.CodigoUnidadeNegocio, i.CodigoIndicador, @AnoAtual, @MesAtual, 'A')
		              FROM {0}.{1}.Indicador i INNER JOIN 
                           {0}.{1}.IndicadorUnidade AS iu ON (iu.CodigoIndicador = i.CodigoIndicador 
                                                            AND iu.CodigoResponsavelIndicadorUnidade = @CodigoUsuario
															AND iu.Meta IS NOT NULL) INNER JOIN 
                           {0}.{1}.UnidadeNegocio AS un ON (un.[CodigoUnidadeNegocio] = iu.[CodigoUnidadeNegocio]
						AND un.DataExclusao	IS NULL
						AND un.[IndicaUnidadeNegocioAtiva]	= 'S'
						AND un.[CodigoEntidade]	= @CodigoEntidadLogada
					)
		             WHERE i.DataExclusao IS NULL 
		               AND iu.DataExclusao IS NULL
                       AND ((GETDATE() BETWEEN IsNull(i.DataInicioValidadeMeta,'01/01/1900') AND IsNull(i.DataTerminoValidadeMeta,'31/12/2999')) OR i.IndicaAcompanhamentoMetaVigencia = 'N')
                       {4}
                 
                IF EXISTS(SELECT 1 FROM  @tbCores)
                    BEGIN
                        INSERT INTO @tbValores
                        SELECT  (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Verde') AS Satisfatorio,
                                 (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Amarelo') AS Atencao,
                                 (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Azul') AS Excelente,
                                 (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Laranja') AS Laranja,
                                 (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Branco') AS Sem_Acompanhamento,
                                 (SELECT Count(1)
                                    FROM @tbCores                            
                                   WHERE Cor = 'Vermelho') AS Critico
                    END
                
            SELECT * FROM @tbValores                
             END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoProjetosGerente(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarLegenda, int codigoUsuario)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

            linkLista = (linkLista == true && podeAcessarLink == true);

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=S&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Excelente"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Satisfatório"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Crítico"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));
            if (mostraLaranja == "S")
            {

                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString(),
                                                                                                               "FF6600",
                                                                                                               link));
            }

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=S&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString(),
                                                                                                           "F3F3F3",
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"FFFFFF\" showBorder=\"0\" BgColor=\"FFFFFF\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"0\" chartLeftMargin=\"0\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"" + mostrarLegenda + "\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"4\" " +
            " showValues=\"1\"  type=\"pie2D\" use3Dlighting=\"0\" legendShadow=\"0\" legendBorderAlpha=\"0\" " + exportar + "showShadow=\"0\"  showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Roboto\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Roboto\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
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

    public string getGraficoDesempenhoProjetosRecurso(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarLegenda, int codigoUsuario)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

            linkLista = (linkLista == true && podeAcessarLink == true);

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=S&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Excelente"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Satisfatório"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Crítico"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));
            if (mostraLaranja == "S")
            {

                if (linkLista)
                    link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Azul=N&Branco=N&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString(),
                                                                                                               "FF6600",
                                                                                                               link));
            }

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=S&CodigoGerente={0}&Programas=N"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString(),
                                                                                                           "F3F3F3",
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"" + mostrarLegenda + "\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"1\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    public string getGraficoDesempenhoMetasAnalista(DataTable dt, string titulo, int fonte, bool linkLista, int codigoUsuario)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = ""; ;
        int i = 0;

        try
        {
            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=N&Azul=S&Branco=N&CR={0}"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Superada"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=S&Amarelo=N&Vermelho=N&Azul=N&Branco=N&CR={0}"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Atingida"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=S&Vermelho=N&Azul=N&Branco=N&CR={0}"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Abaixo(Alerta)"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=S&Azul=N&Branco=N&CR={0}"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Muito Abaixo"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (linkLista)
                link = string.Format(@"link=""F-_top-../../_VisaoExecutiva/Metas/PainelMetas.aspx?Verde=N&Amarelo=N&Vermelho=N&Azul=N&Branco=S&CR={0}"" ", codigoUsuario);

            xmlAux.Append(string.Format(@"<set label=""Desatualizada"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString(),
                                                                                                           "8A8A8A",
                                                                                                           link));

        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        //xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
        //	" chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"4\"" +
        //	" adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
        //	" showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        //xml.Append(xmlAux.ToString());
        //xml.Append("<styles>");
        //xml.Append("<definition>");
        //xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        //xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
        //xml.Append("</definition>");
        //xml.Append("<application>");
        //xml.Append("<apply toObject=\"DATALABELS\" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"DATAVALUES\" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"XAXISNAME \" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"YAXISNAME\" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"YAXISVALUES\" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"TOOLTIP\" styles=\"fontLabels\" />");
        //xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        //xml.Append("</application>");
        //xml.Append("</styles>");
        //xml.Append("</chart>");

        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"FFFFFF\" showBorder=\"0\" BgColor=\"FFFFFF\"" +
            " chartTopMargin=\"5\" showZeroPies=\"0\" chartRightMargin=\"8\" inThousandSeparator=\".\" chartBottomMargin=\"0\" chartLeftMargin=\"0\"" +
            " adjustDiv=\"1\"  slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"4\" " +
            " showValues=\"1\"  type=\"pie2D\" use3Dlighting=\"0\" legendShadow=\"0\" legendBorderAlpha=\"0\" " + exportar + "showShadow=\"0\"  showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontLabels\" type=\"font\" face=\"Roboto\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Roboto\" size=\"" + (fonte + 2) + "\" color=\"000000\" bold=\"0\"/>");
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

    public DataSet getInformacoesPainelOperacional(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int,
			            @codigoTipoAssociacao Int

                SELECT  @codigoTipoAssociacao =  CodigoTipoAssociacao 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = 'MG'
                
                  SET @CodigoUsuarioParam = {3}
                  SET @CodigoEntidadeParam = {2}   
  
                  DECLARE @Contratos TABLE 
                      (
                                    [NumeroContrato]                           Varchar(50)
                                  , [Municipio]                                                Varchar(150)
                                  , [Segmento]                                                 Varchar(50)
                                  , [Status]                                                   Varchar(255)
                                  , [CodigoStatus]                                       int
                                  , [Projeto]                                                  Varchar(255)
                                  , [TipoProjeto]                                        SmallInt
                                  , [VigenteAno]                                         Varchar(3)
                                  , [ValorContrato]                                Decimal(18,4)
                                  , [ValorPago]                                                Decimal(18,4)
                                  , [Saldo]                                                          Decimal(18,4)
                                  , [PercentualFinanceiro]             Decimal(18,5)                
                                  , [PercentualFisico]                       Decimal(18,5)                 
                                  , [Inicio]                                                   DateTime
                                  , [Termino]                                                  DateTime
                                  , [AnoTermino]                                         SmallInt
                                  , [Fornecedor]                                         Varchar(150)
                                  , [TipoObra]                                                 Varchar(50)
                                  , [CodigoProjeto]                                Int
                                  , CodigoObra                                                 Int
                                  , PrevistoPBA                                                Varchar(2000)
                                  , AnoInicio                                                  SmallInt
                                  , InicioVigenciaContrato      Datetime
                                  , TerminoVigenciaContrato     DateTime
                                  , EtapaContratacao                                   Varchar(250)
                                  , CodigoFluxo                                                    Int
                                  , CodigoWorkflow                                     Int
                                  , CodigoInstanciaWf                                  Int
                                  , CodigoEtapaAtual                                   Int
                                  , OcorrenciaAtual                                    Int
                                  , CodigoEtapaInicial                           Int
                                  , QuantidadeObras                                    int
                                  , PrevistoPagamento                              Decimal(18,4)
                                  , NumeroOS                    varchar(64)
                                  , InicioRepactuado            Datetime
                                  , TerminoRepactuado           DateTime 
                                  , TerminoCronograma           DateTime
                                  , ValorMedido                                    Decimal(18,4)   
                                  , IndicaObraServico                                  Char(1)
                                  , UltimaAtualizacaoCronograma DateTime
                                  , UltimaAtualizacaoFoto                  DateTime
                                  , UltimaAnaliseFiscalizacao    DateTime     
                                  , DesempenhoFisico                             Varchar(20)
                                  , SituacaoAtualContrato                  Varchar(40)
                                  , ProcessoComigo                                     Char(3) DEFAULT 'NÃO'
                                  , ProcessoMinhaGerencia                  Char(3) DEFAULT 'NÃO'
                                  , FaixaValor                                                     Varchar(25)
									                , TipoContratacao             Varchar(15)
									                , InicioEtapaAtual                                   DateTime
									                , ObservacaoFluxo                                    Varchar(2000)
                      )
  
                  INSERT INTO @Contratos
                    (
	                        [NumeroContrato], [Municipio], [Segmento], [Status], [CodigoStatus], [Projeto], [TipoProjeto]
                        , [VigenteAno], [ValorContrato], [ValorPago], [Saldo], [PercentualFinanceiro], [PercentualFisico]
                        , [Inicio], [Termino], [AnoTermino], [Fornecedor], [TipoObra], [CodigoProjeto], [CodigoObra]
                        , [PrevistoPBA], [AnoInicio], [InicioVigenciaContrato], [TerminoVigenciaContrato], [EtapaContratacao]
                        , [CodigoFluxo], [CodigoWorkflow], [CodigoInstanciaWf], [CodigoEtapaAtual], [OcorrenciaAtual]
                        , [CodigoEtapaInicial], [QuantidadeObras], [PrevistoPagamento], [NumeroOS], [InicioRepactuado]
                        , [TerminoRepactuado], [TerminoCronograma], [ValorMedido], [IndicaObraServico], [UltimaAtualizacaoCronograma]
                        , [UltimaAtualizacaoFoto], [UltimaAnaliseFiscalizacao], [DesempenhoFisico], [SituacaoAtualContrato]
                        , [ProcessoComigo], [ProcessoMinhaGerencia], [FaixaValor], [TipoContratacao], [InicioEtapaAtual]
                        , [ObservacaoFluxo]
                    )
                    SELECT 
	                        [NumeroContrato], [Municipio], [Segmento], [Status], [CodigoStatus], [Projeto], [TipoProjeto]
                        , [VigenteAno], [ValorContrato], [ValorPago], [Saldo], [PercentualFinanceiro], [PercentualFisico]
                        , [Inicio], [Termino], [AnoTermino], [Fornecedor], [TipoObra], [CodigoProjeto], [CodigoObra]
                        , [PrevistoPBA], [AnoInicio], [InicioVigenciaContrato], [TerminoVigenciaContrato], [EtapaContratacao]
                        , [CodigoFluxo], [CodigoWorkflow], [CodigoInstanciaWf], [CodigoEtapaAtual], [OcorrenciaAtual]
                        , [CodigoEtapaInicial], [QuantidadeObras], [PrevistoPagamento], [NumeroOS], [InicioRepactuado]
                        , [TerminoRepactuado], [TerminoCronograma], [ValorMedido], [IndicaObraServico], [UltimaAtualizacaoCronograma]
                        , [UltimaAtualizacaoFoto], [UltimaAnaliseFiscalizacao], [DesempenhoFisico], [SituacaoAtualContrato]
                        , [ProcessoComigo], [ProcessoMinhaGerencia], [FaixaValor], [TipoContratacao], [InicioEtapaAtual]
                        , [ObservacaoFluxo]
                    FROM {0}.{1}.f_obr_GetDetalhesObras(@CodigoUsuarioParam, @CodigoEntidadeParam)
  
                --Processos Comigo:cDados.getProcessosComigo(codigoEntidade, codigoUsuarioLogado, where);
                SELECT TipoContratacao AS Descricao, COUNT(1) AS Quantidade 
                  FROM @Contratos f
                 WHERE ProcessoComigo = 'SIM'
                 GROUP BY TipoContratacao
            
                --Obras e Serviços em Execução:cDados.getObrasServicosExecucao(codigoEntidade, codigoUsuario, where)
                SELECT DesempenhoFisico AS Descricao, COUNT(1) AS Quantidade 
                  FROM @Contratos f
                 WHERE CodigoStatus = 3
                 GROUP BY DesempenhoFisico



                --Fiscalização: cDados.getFiscalizacoesPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where)
                BEGIN
  
                 DECLARE @diasAtualizacaoCronograma Int 
 
                 SELECT @diasAtualizacaoCronograma = CAST(pcs.[Valor] AS Int) FROM dbo.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasAtualizacaoCronograma'
  
                 IF (@diasAtualizacaoCronograma IS NULL)
                    SET @diasAtualizacaoCronograma = 10   

                 DECLARE @diasAtualizacaoFotos Int 
 
                 SELECT @diasAtualizacaoFotos = CAST(pcs.[Valor] AS Int) FROM dbo.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasAtualizacaoFotos'
  
                 IF (@diasAtualizacaoFotos IS NULL)
                    SET @diasAtualizacaoFotos = 30   

                 DECLARE @diasComentariosFiscalizacao Int 
 
                 SELECT @diasComentariosFiscalizacao = CAST(pcs.[Valor] AS Int) FROM dbo.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoEntidadeParam AND pcs.[Parametro] = 'diasComentariosFiscalizacao'
  
                 IF (@diasComentariosFiscalizacao IS NULL)
                    SET @diasComentariosFiscalizacao = 30       
        
                  SELECT 1 AS Codigo,
                         'Atualizacao Cronograma' AS Descricao,
                         IsNull(SUM(1),0) AS Total
                    FROM @Contratos AS f
                   WHERE CodigoStatus = 3
                     AND (DATEDIFF(DD, f.[UltimaAtualizacaoCronograma], GETDATE()) >= @diasAtualizacaoCronograma
                      OR f.[UltimaAtualizacaoCronograma] IS NULL)
                UNION                           
                  SELECT 2,
                         'Atualizacao Fotos',
                         IsNull(SUM(1),0)
                    FROM @Contratos AS f
                   WHERE CodigoStatus = 3
                     AND (DATEDIFF(DD, f.[UltimaAtualizacaoFoto], GETDATE()) >= @diasAtualizacaoFotos  
                      OR f.[UltimaAtualizacaoFoto] IS NULL)
                UNION                   
                  SELECT 3,
                         'Comentários Fiscalização',
                         IsNull(SUM(1),0)
                    FROM @Contratos AS f
                   WHERE CodigoStatus = 3
                     AND (DATEDIFF(DD, f.[UltimaAnaliseFiscalizacao], GETDATE()) >= @diasComentariosFiscalizacao 
                      OR f.[UltimaAnaliseFiscalizacao] IS NULL)
 
                END

                --Processos em Minha Gerência: cDados.getProcessosMinhaGerencia(int codigoEntidade, int codigoUsuario, string where)

                SELECT TipoContratacao AS Descricao, COUNT(1) AS Quantidade 
                  FROM @Contratos f
                 WHERE ProcessoMinhaGerencia = 'SIM'
                 GROUP BY TipoContratacao
            
                --Contratos:cDados.getContratosSituacaoAtual(int codigoEntidade, int codigoUsuario, string where)
                SELECT SituacaoAtualContrato AS Descricao, COUNT(1) AS Quantidade 
                  FROM @Contratos f
                 WHERE SituacaoAtualContrato <> 'Sem Contrato'
                 GROUP BY SituacaoAtualContrato

                --Comunicação:cDados.getComunicacaoPainelAnalista(int codigoEntidade, int codigoUsuarioLogado, string where)

                            BEGIN
                                --  DECLARE @CodigoUsuarioParam Int,
                              --            @CodigoEntidadeParam Int
                          
                  
                                  SELECT 1 AS Codigo,
					                     'Compromissos' AS Descricao,
                                         IsNull(SUM(1),0) AS Total,
                                         IsNull(SUM(CASE WHEN Convert(Varchar,e.InicioPrevisto,112) < Convert(Varchar,GETDATE()+10,112) THEN 1 ELSE 0 END),0) AS Atrasados
                                    FROM(					
                                        SELECT DataInicio AS InicioPrevisto
                                         FROM {0}.{1}.CompromissoUsuario
                                        WHERE CodigoUsuario = @CodigoUsuarioParam
                                          AND CodigoEntidade = @CodigoEntidadeParam
                		                  UNION
						                SELECT e.InicioPrevisto
						                 FROM {0}.{1}.Evento as e
						                WHERE (e.CodigoEvento IN (SELECT pe.CodigoEvento FROM dbo.ParticipanteEvento pe WHERE pe.CodigoParticipante = @CodigoUsuarioParam)
									                OR e.CodigoResponsavelEvento = @CodigoUsuarioParam)
						                  AND CodigoEntidade = @CodigoEntidadeParam
                                          AND e.TerminoReal IS NULL) AS e                                     
                                                    WHERE (Convert(Varchar,e.InicioPrevisto,112) >= Convert(Varchar,GETDATE(),112))
                                   UNION                   
                                   SELECT 2,
					                      'MensagensRecebidas',
						                  IsNull(SUM(1),0),
						                  IsNull(SUM(CASE WHEN m.IndicaRespostaNecessaria = 'S' THEN 1 ELSE 0 END),0)
                                    FROM {0}.{1}.Mensagem m INNER JOIN
		                                 {0}.{1}.MensagemDestinatario md ON md.CodigoMensagem = m.CodigoMensagem INNER JOIN
		                                 {0}.{1}.Usuario u ON u.CodigoUsuario = m.CodigoUsuarioInclusao INNER JOIN
	                                     {0}.{1}.CaixaMensagem cm ON (cm.CodigoObjetoAssociado = m.CodigoMensagem AND cm.CodigoTipoAssociacao = @codigoTipoAssociacao)
	                               WHERE md.CodigoDestinatario = @CodigoUsuarioParam
	                                 AND md.DataLeitura IS NULL
	                                 AND m.CodigoEntidade = @CodigoEntidadeParam    
                                     AND cm.IndicaTipoMensagem = 'E'  
                                     AND cm.[CodigoUsuario]	= md.[CodigoDestinatario]
                                   UNION                   
                                   SELECT 3,
						                  'Respostas',
						                  IsNull(SUM(1),0),
						                  IsNull(SUM(CASE WHEN Convert(Varchar,m.DataLimiteResposta,112) < Convert(Varchar,GETDATE(),112) THEN 1 ELSE 0 END),0)
                                    FROM {0}.{1}.Mensagem m
	                             WHERE m.CodigoUsuarioInclusao = @CodigoUsuarioParam
	                               AND m.DataResposta IS NULL
	                               AND m.IndicaRespostaNecessaria = 'S'
	                               AND m.CodigoEntidade = @CodigoEntidadeParam
                                             
                                END
                 END           
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getProcessosComigo(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"
            SELECT TipoContratacao AS Descricao, COUNT(1) AS Quantidade 
              FROM {0}.{1}.f_obr_GetDetalhesObras({3}, {2}) f
             WHERE ProcessoComigo = 'SIM'
             GROUP BY TipoContratacao
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getProcessosMinhaGerencia(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"
            SELECT TipoContratacao AS Descricao, COUNT(1) AS Quantidade 
              FROM {0}.{1}.f_obr_GetDetalhesObras({3}, {2}) f
             WHERE ProcessoMinhaGerencia = 'SIM'
             GROUP BY TipoContratacao
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getObrasServicosExecucao(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"
            SELECT DesempenhoFisico AS Descricao, COUNT(1) AS Quantidade 
              FROM {0}.{1}.f_obr_GetDetalhesObras({3}, {2}) f
             WHERE CodigoStatus = 3
             GROUP BY DesempenhoFisico
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getContratosSituacaoAtual(int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
            @"
            SELECT SituacaoAtualContrato AS Descricao, COUNT(1) AS Quantidade 
              FROM {0}.{1}.f_obr_GetDetalhesObras({3}, {2}) f
             WHERE SituacaoAtualContrato <> 'Sem Contrato'
             GROUP BY SituacaoAtualContrato
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficosPainelAnalista2(DataTable dt, string titulo, int fonte, string paramLinkLista, string complementoParamLink)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        string link = "";
        string cor = "";

        try
        {
            foreach (DataRow dr in dt.Rows)
            {
                switch (dr["Descricao"].ToString())
                {
                    case "Muito Atrasado":
                        cor = string.Format(@" color=""{0}"" ", corCritico);
                        break;
                    case "Atrasado":
                        cor = string.Format(@" color=""{0}"" ", corAtencao);
                        break;
                    case "No Prazo":
                        cor = string.Format(@" color=""{0}"" ", corSatisfatorio);
                        break;
                    case "Sem Cronograma":
                        cor = string.Format(@" color=""{0}"" ", corFundoBullets);
                        break;
                    default:
                        cor = "";
                        break;
                }

                if (paramLinkLista != "")
                    link = string.Format(@"link=""F-_top-../../_VisaoNE/ListaObras.aspx?{0}={1}{2}"" ", paramLinkLista, Server.UrlEncode(dr["Descricao"].ToString()), complementoParamLink);

                xmlAux.Append(string.Format(@"<set label=""{0}"" value=""{1}"" {2} {3} />", dr["Descricao"].ToString(),
                                                                                       dr["Quantidade"].ToString(),
                                                                                       link,
                                                                                       cor));
            }
        }
        catch
        {
            return "";
        }

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        //configuração geral do gráfico
        xml.Append("<chart caption=\"" + titulo + "\" canvasBgColor=\"F7F7F7\" showBorder=\"0\" BgColor=\"F7F7F7\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"1\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"1\"" +
            " adjustDiv=\"1\" showZeroPies=\"0\" slantLabels=\"1\" labelDisplay=\"ROTATE\" labelDistance=\"1\" enablesmartlabels=\"0\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " showShadow=\"1\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
        xml.Append(xmlAux.ToString());
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

    #endregion

    #region Painel Integrante Equipe

    public DataSet getListaTarefasIntegranteEquipe(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT CodigoAtribuicao,
                     NomeProjeto,
                     NomeTarefa,
                     InicioPrevisto AS Inicio,
                     TerminoPrevisto AS Termino,
                     PercentualConcluido,
                     CASE WHEN IndicaTarefaCritica = 'S' THEN 'tarefaCritica' ELSE 'vazioPequeno' END AS TarefaCritica,
                     CASE WHEN IndicaTarefaAtrasada = 'S' THEN 'Sim' ELSE 'Não' END AS TarefaAtrasada,
                     Anotacoes,
                     IndicaToDoList
                FROM {0}.{1}.f_getTarefasUsuario({2}, {3})
               WHERE 1 = 1
                 {4}
               ORDER BY 2,4

            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Menu e Acesso

    public DataSet getMenuAcessoUsuario(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int
                          
                  SET @CodigoUsuarioParam = {3}
                  SET @CodigoEntidadeParam = {2}

                  SELECT * 
                    FROM {0}.{1}.f_GetMenu(@CodigoUsuarioParam, @CodigoEntidadeParam, 1) 
                                                      
                                             
                END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIniciaisMenuAcessoUsuario(int codigoEntidade, int codigoUsuarioLogado, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
                  DECLARE @CodigoUsuarioParam Int,
                          @CodigoEntidadeParam Int
                          
                  SET @CodigoUsuarioParam = {3}
                  SET @CodigoEntidadeParam = {2}

                  SELECT Iniciais, NomeMenu 
                    FROM {0}.{1}.f_GetMenu(@CodigoUsuarioParam, @CodigoEntidadeParam, 1) WHERE NivelObjetoMenu = 1                                     
                                             
                END
            ", bancodb, Ownerdb, codigoEntidade, codigoUsuarioLogado, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Navegação Sistema
    //Exclui Níveis no XMX para que mantenha o fluxo de mapeamento correto. Atenção no momento e o nível que deseja limpar.
    public void excluiNiveisAbaixo(int nivel)
    {
        XmlDocument doc = new XmlDocument();

        string caminho = Session["NomeArquivoNavegacao"] + "";

        if (caminho != "")
        {
            try
            {
                doc.Load(caminho);

                int i = 0;
                int niveis = doc.ChildNodes[1].ChildNodes.Count;

                for (i = 0; i < niveis; i++)
                {
                    XmlNode no = doc.SelectSingleNode(String.Format("/caminho/N[nivel={0}]", i));
                    if (no != null)
                    {
                        if (no.SelectSingleNode("./nivel").InnerText != "" && int.Parse(no.SelectSingleNode("./nivel").InnerText) >= nivel)
                            doc.DocumentElement.RemoveChild(no);
                    }
                }

                doc.Save(caminho);
            }
            catch
            {
                Session.Remove("NomeArquivoNavegacao");
            }
        }
    }

    public string getCodigoGlossarioTela(Page page)
    {
        string retorno = "-1";
        string urlTela = "";
        urlTela = page.Request.AppRelativeCurrentExecutionFilePath.Replace("~/", "");

        string comandoSQL = string.Format(@"
        
        SELECT CodigoGlossarioAjuda 
        FROM GlossarioAjuda 
        WHERE CodigoFuncionalidade 
          IN (SELECT CodigoFuncionalidade 
                FROM FuncionalidadePortal
               WHERE URL like '%{0}%')", urlTela);
        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            retorno = ds.Tables[0].Rows[0]["CodigoGlossarioAjuda"].ToString();
        }

        return retorno;
    }

    public string getCodigoGlossarioTela(string urlTela)
    {
        string retorno = "-1";

        string comandoSQL = string.Format(@"
        
        SELECT CodigoGlossarioAjuda 
        FROM GlossarioAjuda 
        WHERE CodigoFuncionalidade 
          IN (SELECT CodigoFuncionalidade 
                FROM FuncionalidadePortal
               WHERE URL like '%{0}%')", urlTela.Replace("\\", "/"));
        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            retorno = ds.Tables[0].Rows[0]["CodigoGlossarioAjuda"].ToString();
        }

        return retorno;
    }

    public DataSet getMenuSistema(int codigoEntidadeLogada, int codigoUsuarioLogado, Page pagina)
    {
        XmlDocument doc = new XmlDocument();
        DataSet dsRetorno = new DataSet();

        string caminho = Session["NomeArquivoMenuMaster" + codigoEntidadeLogada] + "";

        if (caminho != "")
        {
            try
            {
                //doc.Load(caminho);
                dsRetorno.ReadXml(caminho);
            }
            catch
            {
                Session.Remove("NomeArquivoMenuMaster" + codigoEntidadeLogada);
                DataSet dsMenu = getMenuAcessoUsuario(codigoEntidadeLogada, codigoUsuarioLogado, "");
                montaXMLMenu(dsMenu, codigoUsuarioLogado, codigoEntidadeLogada, pagina);
                return dsMenu;
            }
        }
        else
        {
            DataSet dsMenu = getMenuAcessoUsuario(codigoEntidadeLogada, codigoUsuarioLogado, "");
            montaXMLMenu(dsMenu, codigoUsuarioLogado, codigoEntidadeLogada, pagina);
            return dsMenu;
        }

        return dsRetorno;
    }

    public void montaXMLMenu(DataSet dsMenu, int idUsuarioLogado, int codigoEntidade, Page pagina)
    {
        string nomeArquivo = "/ArquivosTemporarios/xml_MENU" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + "_" + idUsuarioLogado + "_" + codigoEntidade + ".xml"; ;

        string xml = dsMenu.GetXml();

        Session["NomeArquivoMenuMaster" + codigoEntidade] = pagina.Request.PhysicalApplicationPath + nomeArquivo;

        escreveXML(xml, nomeArquivo);
    }

    public string GetQueryStringValueFromRawUrl(string queryStringKey)
    {
        var currentUri = new Uri(HttpContext.Current.Request.Url.Scheme + "://" +
            HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port +
            HttpContext.Current.Request.RawUrl);
        var queryStringCollection = HttpUtility.ParseQueryString((currentUri).Query);
        return queryStringCollection.Get(queryStringKey);
    }

    public string insereNivel(int nivelTela, Page page)
    {
        XmlDocument doc = new XmlDocument();

        //Pega o Nome da Tela Inicial
        var dsTelaUrlPadrao = GetNomeTelaUrlPadraoUsuarioIdioma(getInfoSistema("IDUsuarioLogado").ToString(), getInfoSistema("CodigoEntidade").ToString());

        string nomeArquivo = "/ArquivosTemporarios/xml_Caminho" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + "_" + getInfoSistema("IDUsuarioLogado").ToString() + ".xml";

        string nomeTela = dsTelaUrlPadrao.Tables[0].Rows[0]["NomeObjetoMenu"].ToString(), urlTela, parametrosTela;

        urlTela = page.Request.AppRelativeCurrentExecutionFilePath.Replace("~/", "");
        var queryString = page.Request.QueryString;
        var queryStringVariables = queryString.AllKeys.Where(key => !string.IsNullOrEmpty(queryString[key])).Select(
            key => string.Format("{0}={1}", key, page.Server.UrlEncode(page.Server.UrlDecode(queryString[key]))));
        parametrosTela = string.Join("&", queryStringVariables);

        var idioma = System.Threading.Thread.CurrentThread.CurrentCulture.ToString().ToLower();
        var colunaNomeObjeto = idioma.StartsWith("pt") ? "NomeObjetoMenu_PT" : "NomeObjetoMenu_EN";
        string comandoSQL = string.Format(@"  SELECT ome.{2}, om.Iniciais 
	                                            FROM ObjetoMenu om INNER JOIN 
			                                             ObjetoMenuEntidade ome ON (ome.CodigoObjetoMenu = om.CodigoObjetoMenu
															                    AND ome.CodigoEntidade = {0})
                                             WHERE URLObjetoMenu IS NOT NULL
                                               AND URLObjetoMenu LIKE '~/{1}%'", getInfoSistema("CodigoEntidade"), urlTela, colunaNomeObjeto);

        DataSet ds = getDataSet(comandoSQL);

        Control label;

        if (page.Form.FindControl("AreaTrabalho") != null)
        {
            label = page.Form.FindControl("AreaTrabalho").FindControl("lblTituloTela");
        }
        else
        {
            label = page.FindControl("lblTituloTela");
        }


        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            if (ds.Tables[0].Rows.Count == 1)
                nomeTela = ds.Tables[0].Rows[0][colunaNomeObjeto].ToString();
            else if (page.Request.QueryString["TITULO"] != null)
            {
                /*
                 * IMPORTANTE: (WI 3688) O título "Relatórios+de+Projetos" na página "http://localhost:42388/_Processos/Visualizacao/index.aspx?tipo=R&cmm=PRJ&TITULO=Relat%C3%B3rios+de+Projetos"
                 * exibia caracteres estranhos na palavra "Relatórios" por conta do caractere acentuado ó agudo. Há problemas de codficação nos parâmetros QueryString com caracteres acentuados.
                 * A solução foi criar a função GetQueryStringValueFromRawUrl(), a qual deverá ser usada no lugar da obtenção direta dos parâmetros QueryString no objeto "page.Request".
                */
                //nomeTela = GetQueryStringValueFromRawUrl("TITULO");
                nomeTela = Server.UrlDecode(page.Request.QueryString["TITULO"]);
                //nomeTela = page.Server.UrlDecode(page.Request.QueryString["TITULO"].ToString());
                //nomeTela = System.Text.Encoding.GetEncoding("ISO-8859-1").GetString(System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.GetEncoding("ISO-8859-1"), System.Text.Encoding.UTF8.GetBytes(nomeTela)));
                //nomeTela = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.UTF8, System.Text.Encoding.UTF8.GetBytes(nomeTela)));
            }
            else if (page.Request.QueryString["TipoTela"] != null)
            {
                if (page.Request.QueryString["TipoTela"].ToString() == "Q")
                {
                    DataRow[] drs = ds.Tables[0].Select("Iniciais = 'ISSUE'");

                    if (drs.Length > 0)
                        nomeTela = drs[0][colunaNomeObjeto].ToString();
                }
                else if (page.Request.QueryString["TipoTela"].ToString() == "R")
                {
                    DataRow[] drs = ds.Tables[0].Select("Iniciais = 'RISCO'");

                    if (drs.Length > 0)
                        nomeTela = drs[0][colunaNomeObjeto].ToString();
                }
            }
        }
        else
        {
            if (label != null)
            {
                if (label is ASPxLabel)
                    nomeTela = (label as ASPxLabel).Text;
                else if (label is Label)
                    nomeTela = (label as Label).Text;
            }
        }

        if (label != null && nomeTela != "")
        {
            if (label is ASPxLabel)
                (label as ASPxLabel).Text = nomeTela;
            else if (label is Label)
                (label as Label).Text = nomeTela;
        }

        string caminho = Session["NomeArquivoNavegacao"] + "";

        if (caminho != "")
        {
            try
            {
                doc.Load(caminho);

                XmlNode linha = doc.CreateElement("N");

                XmlNode id = doc.CreateElement("id");
                XmlNode nivel = doc.CreateElement("nivel");
                XmlNode url = doc.CreateElement("url");
                XmlNode nome = doc.CreateElement("nome");
                XmlNode parametros = doc.CreateElement("parametros");

                id.InnerText = doc.SelectSingleNode("/caminho").ChildNodes.Count.ToString();
                nivel.InnerText = doc.SelectSingleNode("/caminho").ChildNodes.Count.ToString();
                url.InnerText = urlTela.Replace("&", "&amp;");
                nome.InnerText = nomeTela;
                parametros.InnerText = parametrosTela;

                linha.AppendChild(id);
                linha.AppendChild(nivel);
                linha.AppendChild(url);
                linha.AppendChild(nome);
                linha.AppendChild(parametros);

                doc.SelectSingleNode("/caminho").AppendChild(linha);

                doc.Save(caminho);
            }
            catch
            {
                Session.Remove("NomeArquivoNavegacao");
            }
        }

        return nomeTela;
    }

    public string insereNivelPrerencia(int nivelTela)
    {
        XmlDocument doc = new XmlDocument();
        var dsTelaNomePadrao = GetNomeTelaUrlPadraoUsuarioIdioma(getInfoSistema("IDUsuarioLogado").ToString(), getInfoSistema("CodigoEntidade").ToString());
        {
            string urlDestino = dsTelaNomePadrao.Tables[0].Rows[0]["URLObjetoMenu"].ToString(); ;
            string nomeTela = dsTelaNomePadrao.Tables[0].Rows[0]["NomeObjetoMenu"].ToString();
            string nomeArquivo = "/ArquivosTemporarios/xml_Caminho" + DateTime.Now.ToString().Replace("/", "-").Replace(":", "_") + "_" + getInfoSistema("IDUsuarioLogado").ToString() + ".xml";
            string caminho = Session["NomeArquivoNavegacao"] + "";
            if (caminho != "")
            {
                try
                {
                    doc.Load(caminho);

                    XmlNode linha = doc.CreateElement("N");

                    XmlNode id = doc.CreateElement("id");
                    XmlNode nivel = doc.CreateElement("nivel");
                    XmlNode url = doc.CreateElement("url");
                    XmlNode nome = doc.CreateElement("nome");
                    XmlNode parametros = doc.CreateElement("parametros");

                    id.InnerText = doc.SelectSingleNode("/caminho").ChildNodes.Count.ToString();
                    nivel.InnerText = doc.SelectSingleNode("/caminho").ChildNodes.Count.ToString();
                    url.InnerText = urlDestino.Replace("&", "&amp;");
                    nome.InnerText = nomeTela;
                    linha.AppendChild(id);
                    linha.AppendChild(nivel);
                    linha.AppendChild(url);
                    linha.AppendChild(nome);
                    linha.AppendChild(parametros);
                    doc.SelectSingleNode("/caminho").AppendChild(linha);
                    doc.Save(caminho);
                }
                catch
                {
                    Session.Remove("NomeArquivoNavegacao");
                }
            }

            return nomeTela;
        }
    }

    public DataSet getFavoritosUsuario(int codigoEntidade, int codigoUsuarioLogado)
    {
        string comandoSQL = string.Format(@"
                  BEGIN

	                DECLARE @CodigoUsuario int,
					                @CodigoEntidade int
					
	                DECLARE @tbLinksDisponiveis TABLE(CodigoLink int)
	
	                SET @CodigoUsuario = {2}
	                SET @CodigoEntidade = {3}
	
	                INSERT INTO @tbLinksDisponiveis
		                SELECT lfu.CodigoLinkFavorito 
			              FROM {0}.{1}.LinkFavoritoUsuario lfu
		                 WHERE lfu.CodigoEntidade = @CodigoEntidade
		                   AND lfu.CodigoUsuario = @CodigoUsuario
		                   AND lfu.IniciaisTipoObjeto <> 'PROJ'	
		   
	                INSERT INTO @tbLinksDisponiveis
		                SELECT lfu.CodigoLinkFavorito 
			              FROM {0}.{1}.LinkFavoritoUsuario lfu
		                 WHERE lfu.CodigoEntidade = @CodigoEntidade
		                   AND lfu.CodigoUsuario = @CodigoUsuario
		                   AND lfu.IniciaisTipoObjeto = 'PROJ'
		                   AND lfu.CodigoObjetoAssociado IN(SELECT CodigoProjeto FROM {0}.{1}.Projeto p WHERE p.DataExclusao IS NULL)

	                SELECT CodigoLinkFavorito, NomeLinkFavorito, URL, NomeTelaReferencia 
		                FROM {0}.{1}.LinkFavoritoUsuario
	                 WHERE CodigoLinkFavorito IN(SELECT CodigoLink FROM @tbLinksDisponiveis)
	                 ORDER BY NomeLinkFavorito 
                   
                 END     
            ", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public bool verificaExistenciaFavoritosUsuario(int codigoEntidade, int codigoUsuarioLogado, string iniciaisMenu, string iniciaisTipoObjeto, int codigoObjetoAssociado)
    {
        string comandoSQL = string.Format(@"
                  SELECT ISNULL(COUNT(1), 0)
                    FROM {0}.{1}.LinkFavoritoUsuario
                   WHERE CodigoUsuario = {2}                                      
                     AND CodigoEntidade = {3}
                     AND IniciaisMenu = '{4}'
                     AND IniciaisTipoObjeto = '{5}'
                     AND CodigoObjetoAssociado = {6}    
            ", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, iniciaisMenu, iniciaisTipoObjeto, codigoObjetoAssociado);

        bool retorno = false;

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && int.Parse(ds.Tables[0].Rows[0][0].ToString()) > 0)
            retorno = true;


        return retorno;
    }

    public bool incluiFavoritoUsuario(int codigoEntidade, int codigoUsuarioLogado, string nomeLinkFavorito, string nomeTelaReferencia, string url, string iniciaisMenu, string iniciaisTipoObjeto, int codigoObjetoAssociado, ref string mensagemErro)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            INSERT INTO {0}.{1}.LinkFavoritoUsuario(CodigoUsuario, CodigoEntidade, NomeLinkFavorito, NomeTelaReferencia, URL, IniciaisMenu, IniciaisTipoObjeto, CodigoObjetoAssociado)
                                             VALUES({2}, {3}, '{4}', '{9}', '{5}', '{6}', '{7}', {8})
        ", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, nomeLinkFavorito, url, iniciaisMenu, iniciaisTipoObjeto, codigoObjetoAssociado, nomeTelaReferencia);

        int regAfetados = 0;

        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;

    }


    public bool excluiFavoritoUsuario(int codigoEntidade, int codigoUsuarioLogado, string iniciaisMenu, string iniciaisTipoObjeto, int codigoObjetoAssociado, ref string mensagemErro)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            DELETE FROM {0}.{1}.LinkFavoritoUsuario
                  WHERE CodigoUsuario = {2}                                      
                         AND CodigoEntidade = {3}
                         AND IniciaisMenu = '{4}'
                         AND IniciaisTipoObjeto = '{5}'
                         AND CodigoObjetoAssociado = {6}  
        ", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, iniciaisMenu, iniciaisTipoObjeto, codigoObjetoAssociado);

        int regAfetados = 0;

        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;

    }

    #endregion

    #region _Projeto - Administracao - StatusReport.aspx

    public DataSet getModeloStatusReport(int codigoEntidadeUsuarioResponsavel, string where)
    {
        string comandoSQL = string.Format(@"
 SELECT MSR.CodigoModeloStatusReport,
        MSR.DescricaoModeloStatusReport,
        MSR.CodigoEntidade,
        TP.CodigoPeriodicidade,
        TP.DescricaoPeriodicidade_PT,
        MSR.ListaTarefasAtrasadas,
        MSR.ListaTarefasConcluidas,
        MSR.ListaTarefasFuturas,
        MSR.ListaMarcosConcluidos,
        MSR.ListaMarcosAtrasados,
        MSR.ListaMarcosFuturos,
        MSR.GraficoDesempenhoFisico,
        MSR.ListaRH,
        MSR.GraficoDesempenhoCusto,
        MSR.ListaContasCusto,
        MSR.GraficoDesempenhoReceita,
        MSR.ListaContasReceita,
        MSR.AnaliseValorAgregado,
        MSR.ListaRiscosAtivos,
        MSR.ListaRiscosEliminados,
        MSR.ListaQuestoesAtivas,
        MSR.ListaQuestoesResolvidas,
        MSR.ListaMetasResultados,
        MSR.ListaPendenciasToDoList,
        MSR.ComentarioGeral,
        MSR.ComentarioFisico,
        MSR.ComentarioFinanceiro,
        MSR.ComentarioRisco,
        MSR.ComentarioQuestao,
        MSR.ComentarioMeta,
        MSR.ListaContratos,
		MSR.ListaEntregas,
        MSR.ComentarioPlanoAcao,
        MSR.ToleranciaPeriodicidade,
        MSR.IniciaisModeloControladoSistema
   FROM {0}.{1}.ModeloStatusReport MSR INNER JOIN
        {0}.{1}.TipoPeriodicidade TP ON TP.CodigoPeriodicidade = CodigoPeriodicidadeGeracao
  WHERE MSR.CodigoEntidade = {2}
    AND MSR.DataDesativacao IS NULL
    AND MSR.DataExclusao IS NULL
  ORDER BY MSR.DescricaoModeloStatusReport ASC
    {3}
 ", bancodb, Ownerdb, codigoEntidadeUsuarioResponsavel, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getModeloStatusReportPorObjeto(long codigoObjeto, string iniciaisTipoAssociacaoObjeto)
    {
        string comandoSQL = string.Format(@"
 SELECT MSR.CodigoModeloStatusReport,
        MSR.DescricaoModeloStatusReport,
        MSR.CodigoEntidade,
        TP.CodigoPeriodicidade,
        TP.DescricaoPeriodicidade_PT,
        MSR.ListaTarefasAtrasadas,
        MSR.ListaTarefasConcluidas,
        MSR.ListaTarefasFuturas,
        MSR.ListaMarcosConcluidos,
        MSR.ListaMarcosAtrasados,
        MSR.ListaMarcosFuturos,
        MSR.GraficoDesempenhoFisico,
        MSR.ListaRH,
        MSR.GraficoDesempenhoCusto,
        MSR.ListaContasCusto,
        MSR.GraficoDesempenhoReceita,
        MSR.ListaContasReceita,
        MSR.AnaliseValorAgregado,
        MSR.ListaRiscosAtivos,
        MSR.ListaRiscosEliminados,
        MSR.ListaQuestoesAtivas,
        MSR.ListaQuestoesResolvidas,
        MSR.ListaMetasResultados,
        MSR.ListaPendenciasToDoList,
        MSR.ComentarioGeral,
        MSR.ComentarioFisico,
        MSR.ComentarioFinanceiro,
        MSR.ComentarioRisco,
        MSR.ComentarioQuestao,
        MSR.ComentarioMeta,
        MSR.ListaContratos,
        MSR.ComentarioPlanoAcao,
        MSR.ToleranciaPeriodicidade,
        MSR.IniciaisModeloControladoSistema,
        CASE WHEN GR.DataAtivacao IS NULL THEN 'N' ELSE 'S' END AS IndicaIncluiGraficoReceita
   FROM {0}.{1}.ModeloStatusReport AS MSR INNER JOIN
        {0}.{1}.TipoPeriodicidade AS TP ON TP.CodigoPeriodicidade = CodigoPeriodicidadeGeracao INNER JOIN
        {0}.{1}.ModeloStatusReportObjeto AS MSRO ON MSRO.CodigoModeloStatusReport = MSR.CodigoModeloStatusReport AND
                                                    MSRO.IndicaModeloAtivoObjeto = 'S' AND 
                                                    MSRO.CodigoObjeto = {2} AND 
                                                    MSRO.CodigoTipoAssociacaoObjeto = {0}.{1}.f_GetCodigoTipoAssociacao('{3}') LEFT JOIN
        {0}.{1}.GraficoReceitaNoRAP AS GR ON GR.CodigoModeloStatusReport = MSRO.CodigoModeloStatusReport AND
                                             GR.CodigoObjeto = MSRO.CodigoObjeto AND
                                             GR.CodigoTipoAssociacaoObjeto = MSRO.CodigoTipoAssociacaoObjeto AND
                                             GR.DataDesativacao IS NULL
  WHERE MSR.DataDesativacao IS NULL
    AND MSR.DataExclusao IS NULL"
            , bancodb
            , Ownerdb
            , codigoObjeto
            , iniciaisTipoAssociacaoObjeto);

        return getDataSet(comandoSQL);
    }
    public DataSet getHistoricoMedicaoApontamento(int codigoEntidadeUsuarioResponsavel, int codigoUsuario, int idProjeto)
    {
        string comandoSQL = string.Format(@"

        SELECT SequenciaBoletimProjeto
                , CodigoProjeto
                , CodigoBoletim
                , AnoPeriodoBoletim
                , MesPeriodoBoletim
                , DataGeracaoBoletim
                , CodigoUsuarioGeracao
                , NomeUsuarioGeracao
                , IndicaBoletimExcluido
                , DescricaoPeriodo
            FROM {0}.{1}.f_art_GetListaBoletinsMedicao({2},{3},{4}) order by DataGeracaoBoletim desc", bancodb, Ownerdb, codigoEntidadeUsuarioResponsavel, codigoUsuario, idProjeto);

        return getDataSet(comandoSQL);
    }

    public DataSet getHistoricoStatusReport(int codigoEntidadeUsuarioResponsavel, int codigoObjeto, string iniciaisTipoAssociacao)
    {
        string comandoSQL = string.Format(@"
DECLARE @CodigoEntidade INT
DECLARE @CodigoObjeto INT
DECLARE @CodigoTipoAssociacao INT
	SET @CodigoEntidade = {2}
	SET @CodigoObjeto = {3}	
	SET @CodigoTipoAssociacao = {0}.{1}.f_GetCodigoTipoAssociacao('{4}');

 SELECT SR.CodigoStatusReport
       ,{0}.{1}.f_GetDescricaoStatusReport(SR.CodigoStatusReport) NomeRelatorio 
       ,SR.CodigoStatusReport
       ,SR.CodigoObjeto
       ,SR.DataGeracao
       ,SR.DataPublicacao
       ,SR.DataEnvioDestinatarios
       ,AP.CodigoAnalisePerformance
       ,AP.IndicaRegistroEditavel
       ,AP.Analise AS ComentarioGeral--,SR.ComentarioGeral
       ,SR.ComentarioFisico
       ,SR.ComentarioFinanceiro
       ,SR.ComentarioRisco
       ,SR.ComentarioQuestao
       ,SR.ComentarioMeta
       ,SR.ComentarioPlanoAcao
       ,MSR.ComentarioGeral AS MostraComentarioGeral
       ,MSR.ComentarioFisico AS MostraComentarioFisico 
       ,MSR.ComentarioFinanceiro AS MostraComentarioFinanceiro 
       ,MSR.ComentarioRisco AS MostraComentarioRisco 
       ,MSR.ComentarioQuestao AS MostraComentarioQuestao 
       ,MSR.ComentarioMeta AS MostraComentarioMeta
       ,MSR.CodigoModeloStatusReport
       ,MSR.DescricaoModeloStatusReport
       ,MSR.ComentarioPlanoAcao AS MostraComentarioPlanoAcao
       ,(SELECT CASE WHEN COUNT(*) > 0 THEN 'S' ELSE 'N' END
		   FROM {0}.{1}.DestinatarioModeloStatusReport DMSR 
		  WHERE DMSR.CodigoObjeto = SR.CodigoObjeto
			AND DMSR.CodigoTipoAssociacaoObjeto = SR.CodigoTipoAssociacaoObjeto 
			AND DMSR.CodigoModeloStatusReport = SR.CodigoModeloStatusReport) AS PossuiDestinatarios
       ,MSR.IniciaisModeloControladoSistema
       ,SR.DestaquesMes
   FROM {0}.{1}.StatusReport SR INNER JOIN
		{0}.{1}.ModeloStatusReport MSR ON MSR.CodigoModeloStatusReport = SR.CodigoModeloStatusReport LEFT JOIN
		{0}.{1}.AnalisePerformance AP ON AP.CodigoAnalisePerformance = SR.CodigoAnalisePerformance
  WHERE MSR.DataExclusao IS NULL
	AND MSR.CodigoEntidade = @CodigoEntidade
    AND SR.DataExclusao IS NULL
	AND SR.CodigoObjeto = @CodigoObjeto
	AND SR.CodigoTipoAssociacaoObjeto = @CodigoTipoAssociacao
	AND (SR.CodigoStatusReportSuperior IS NULL OR SR.CodigoStatusReportSuperior = SR.CodigoStatusReport)
  ORDER BY
		SR.DataGeracao DESC
 ", bancodb, Ownerdb, codigoEntidadeUsuarioResponsavel, codigoObjeto, iniciaisTipoAssociacao);

        return getDataSet(comandoSQL);
    }

    public bool incluiuModeloStatusReport(int codigoEntidade, string DescricaoModeloStatusReport,
           int codigoUsuarioInclusao, char listaTarefasAtrasadas, char listaTarefasConcluidas,
           char listaTarefasFuturas, char listaMarcosConcluidos, char listaMarcosAtrasados,
           char listaMarcosFuturos, char graficoDesempenhoFisico, char listaRH, char graficoDesempenhoCusto,
           char listaContasCusto, char graficoDesempenhoReceita, char listaContasReceita, char analiseValorAgregado,
           char listaRiscosAtivos, char listaRiscosEliminados, char listaQuestoesAtivas, char listaQuestoesResolvidas,
           char listaMetasResultados, char listaPendenciasToDoList, char comentarioGeral, char comentarioFisico,
           char comentarioFinanceiro, char comentarioRisco, char comentarioQuestao, char comentarioMeta,
           char listaContratos, char planoAcaoGeral, char listaEntregas, int codigoPeriodicidadeGeracao, int toleranciaPeriodicidade, ref int codigoNovoModeloStatusReport, bool indicaPadraoNovo)
    {
        bool retorno = true;
        string comandoSQL = string.Empty;
        try
        {
            #region comandoSql

            comandoSQL = string.Format(@"
BEGIN

DECLARE @CodigoModeloStatus int

 INSERT INTO {0}.{1}.ModeloStatusReport
           (DescricaoModeloStatusReport,
           CodigoEntidade,
           DataInclusao,
           CodigoUsuarioInclusao,
           DataUltimaAlteracao,
           CodigoUsuarioUltimaAlteracao,
           DataExclusao,
           CodigoUsuarioExclusao,
           ListaTarefasAtrasadas,
           ListaTarefasConcluidas,
           ListaTarefasFuturas,
           ListaMarcosConcluidos,
           ListaMarcosAtrasados,
           ListaMarcosFuturos,
           GraficoDesempenhoFisico,
           ListaRH,
           GraficoDesempenhoCusto,
           ListaContasCusto,
           GraficoDesempenhoReceita,
           ListaContasReceita,
           AnaliseValorAgregado,
           ListaRiscosAtivos,
           ListaRiscosEliminados,
           ListaQuestoesAtivas,
           ListaQuestoesResolvidas,
           ListaMetasResultados,
           ListaPendenciasToDoList,
           ComentarioGeral,
           ComentarioFisico,
           ComentarioFinanceiro,
           ComentarioRisco,
           ComentarioQuestao,
           ComentarioMeta,
           CodigoPeriodicidadeGeracao,
           ListaContratos,
           ComentarioPlanoAcao,
		   ListaEntregas,
           ToleranciaPeriodicidade,
		   IniciaisModeloControladoSistema)
     VALUES
           ('{3}'
           ,'{2}'
           ,GETDATE()
           ,{4}
           ,NULL
           ,NULL
           ,NULL
           ,NULL
           ,'{5}'
           ,'{6}'
           ,'{7}'
           ,'{8}'
           ,'{9}'
           ,'{10}'
           ,'{11}'
           ,'{12}'
           ,'{13}'
           ,'{14}'
           ,'{15}'
           ,'{16}'
           ,'{17}'
           ,'{18}'
           ,'{19}'
           ,'{20}'
           ,'{21}'
           ,'{22}'
           ,'{23}'
           ,'{24}'
           ,'{25}'
           ,'{26}'
           ,'{27}'
           ,'{28}'
           ,'{29}'
           ,{30}
           ,'{31}'
           ,'{32}'
           ,'{33}'
		   ,{34}
           ,{35})

	SET @CodigoModeloStatus = scope_identity()

 SELECT @CodigoModeloStatus AS CodigoModeloStatusReport

END", bancodb, Ownerdb, codigoEntidade, DescricaoModeloStatusReport,
    codigoUsuarioInclusao, listaTarefasAtrasadas, listaTarefasConcluidas,
    listaTarefasFuturas, listaMarcosConcluidos, listaMarcosAtrasados,
    listaMarcosFuturos, graficoDesempenhoFisico, listaRH, graficoDesempenhoCusto,
    listaContasCusto, graficoDesempenhoReceita, listaContasReceita, analiseValorAgregado,
    listaRiscosAtivos, listaRiscosEliminados, listaQuestoesAtivas, listaQuestoesResolvidas,
    listaMetasResultados, listaPendenciasToDoList, comentarioGeral, comentarioFisico,
    comentarioFinanceiro, comentarioRisco, comentarioQuestao, comentarioMeta,
    codigoPeriodicidadeGeracao, listaContratos, planoAcaoGeral, listaEntregas,
    toleranciaPeriodicidade, (indicaPadraoNovo ? "'PADRAONOVO'" : "NULL"));

            #endregion

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                codigoNovoModeloStatusReport = int.Parse(ds.Tables[0].Rows[0]["CodigoModeloStatusReport"].ToString());
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao incluir o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL); ;
        }
        return retorno;
    }

    public bool atualizaModeloStatusReport(int codigoEntidade, string DescricaoModeloStatusReport,
           int codigoUsuarioAlteracao, char listaTarefasAtrasadas, char listaTarefasConcluidas,
           char listaTarefasFuturas, char listaMarcosConcluidos, char listaMarcosAtrasados,
           char listaMarcosFuturos, char graficoDesempenhoFisico, char listaRH, char graficoDesempenhoCusto,
           char listaContasCusto, char graficoDesempenhoReceita, char listaContasReceita, char analiseValorAgregado,
           char listaRiscosAtivos, char listaRiscosEliminados, char listaQuestoesAtivas, char listaQuestoesResolvidas,
           char listaMetasResultados, char listaPendenciasToDoList, char comentarioGeral, char comentarioFisico,
           char comentarioFinanceiro, char comentarioRisco, char comentarioQuestao, char comentarioMeta,
           char listaContratos, char planoAcaoGeral, char listaEntregas, int codigoPeriodicidadeGeracao, int toleranciaPeriodicidade, int codigoModeloStatusReport)
    {
        string comandoSQL = "";
        int regAfetados = 0;

        try
        {
            #region comandoSql
            comandoSQL = string.Format(@"
UPDATE {0}.{1}.ModeloStatusReport
   SET DescricaoModeloStatusReport = '{3}'
      ,[CodigoEntidade] = {2}
      ,[DataUltimaAlteracao] = GETDATE()
      ,[CodigoUsuarioUltimaAlteracao] = {4}
      ,[ListaTarefasAtrasadas] = '{5}'
      ,[ListaTarefasConcluidas] = '{6}'
      ,[ListaTarefasFuturas] = '{7}'
      ,[ListaMarcosConcluidos] = '{8}'
      ,[ListaMarcosAtrasados] = '{9}'
      ,[ListaMarcosFuturos] = '{10}'
      ,[GraficoDesempenhoFisico] = '{11}'
      ,[ListaRH] = '{12}'
      ,[GraficoDesempenhoCusto] = '{13}'
      ,[ListaContasCusto] = '{14}'
      ,[GraficoDesempenhoReceita] = '{15}'
      ,[ListaContasReceita] = '{16}'
      ,[AnaliseValorAgregado] = '{17}'
      ,[ListaRiscosAtivos] = '{18}'
      ,[ListaRiscosEliminados] = '{19}'
      ,[ListaQuestoesAtivas] = '{20}'
      ,[ListaQuestoesResolvidas] = '{21}'
      ,[ListaMetasResultados] = '{22}'
      ,[ListaPendenciasToDoList] = '{23}'
      ,[ComentarioGeral] = '{24}'
      ,[ComentarioFisico] = '{25}'
      ,[ComentarioFinanceiro] = '{26}'
      ,[ComentarioRisco] = '{27}'
      ,[ComentarioQuestao] = '{28}'
      ,[ComentarioMeta] = '{29}'
      ,[CodigoPeriodicidadeGeracao] = {30}
      ,[ListaContratos] = '{32}'
      ,[ComentarioPlanoAcao] = '{33}'
	  ,[ListaEntregas] = '{34}'
      ,[ToleranciaPeriodicidade] = {35}
 WHERE CodigoModeloStatusReport = {31}",
                bancodb, Ownerdb, codigoEntidade, DescricaoModeloStatusReport,
                codigoUsuarioAlteracao, listaTarefasAtrasadas, listaTarefasConcluidas,
                listaTarefasFuturas, listaMarcosConcluidos, listaMarcosAtrasados,
                listaMarcosFuturos, graficoDesempenhoFisico, listaRH, graficoDesempenhoCusto,
                listaContasCusto, graficoDesempenhoReceita, listaContasReceita, analiseValorAgregado,
                listaRiscosAtivos, listaRiscosEliminados, listaQuestoesAtivas, listaQuestoesResolvidas,
                listaMetasResultados, listaPendenciasToDoList, comentarioGeral, comentarioFisico,
                comentarioFinanceiro, comentarioRisco, comentarioQuestao, comentarioMeta,
                codigoPeriodicidadeGeracao, codigoModeloStatusReport, listaContratos, planoAcaoGeral, listaEntregas, toleranciaPeriodicidade);
            #endregion

            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool excluiModeloStatusReport(int codigoModeloStatusReport, int codigoUsuarioExclusao)
    {
        string comandoSQL = "";
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
            UPDATE {0}.{1}.ModeloStatusReport
            SET
            DataExclusao = GETDATE(),
            CodigoUsuarioExclusao = {2}
            WHERE CodigoModeloStatusReport = {3}"
                , bancodb, Ownerdb, codigoUsuarioExclusao, codigoModeloStatusReport);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void atualizaDestaqueStatusReport(int codigoStatusReport, int codigoUsuarioResponsavel, string destaque)
    {
        int registrosAfetados = 0;
        string comandoSql = string.Format(@"
                 UPDATE {0}.{1}.[StatusReport]
                    SET [DestaquesMes] = '{3}'
                  WHERE [CodigoStatusReport] = {2}"
            , bancodb, Ownerdb, codigoStatusReport, destaque);
        classeDados.execSQL(comandoSql, ref registrosAfetados);
    }

    public void atualizaComentariosStatusReport(int codigoStatusReport, int codigoUsuarioResponsavel, string comentariosGerais, string analiseDesempenhoFisico, string analiseDesempenhoFinanceiro, string analiseRiscos, string analiseQuestoes, string analiseMetas, string comentarioPlanoAcao)
    {
        int registrosAfetados = 0;
        string comandoSql = string.Format(@"
DECLARE @CodigoStatusReport INT,
        @CodigoObjetoAssociado INT, 
        @CodigoTipoAssociacao INT, 
        @Analise_Original VARCHAR(Max), 
        @Analise VARCHAR(Max), 
        @ComentarioFisico VARCHAR(2000), 
        @ComentarioFinanceiro VARCHAR(2000), 
        @ComentarioRisco VARCHAR(2000), 
        @ComentarioQuestao VARCHAR(2000), 
        @ComentarioMeta VARCHAR(2000), 
        @ComentarioPlanoAcao VARCHAR(2000), 
        @CodigoAnalisePerformace_Original INT,
        @CodigoAnalisePerformace INT,
        @CodigoUsuario INT,
        @Data DATETIME
        
    SET @CodigoStatusReport = {2}
    SET @Analise = '{3}'
    SET @ComentarioFisico = '{4}'
    SET @ComentarioFinanceiro = '{5}'
    SET @ComentarioRisco = '{6}'
    SET @ComentarioQuestao = '{7}'
    SET @ComentarioMeta = '{8}'
    SET @ComentarioPlanoAcao = '{10}'
    SET @CodigoUsuario = {9}
    SET @Data = GETDATE()
    
 SELECT @CodigoAnalisePerformace_Original = sr.CodigoAnalisePerformance,
        @CodigoObjetoAssociado = sr.CodigoObjeto,
        @CodigoTipoAssociacao = sr.CodigoTipoAssociacaoObjeto,
        @Analise_Original = ap.Analise
   FROM {0}.{1}.[StatusReport] sr LEFT JOIN
        {0}.{1}.[AnalisePerformance] ap ON ap.CodigoAnalisePerformance = sr.CodigoAnalisePerformance
  WHERE sr.CodigoStatusReport = @CodigoStatusReport

 UPDATE {0}.{1}.[AnalisePerformance]
    SET [DataAnalisePerformance] = @Data,
        [Analise] = @Analise,
        [CodigoUsuarioUltimaAlteracao] = @CodigoUsuario,
        [DataUltimaAlteracao] = @Data
  WHERE [CodigoAnalisePerformance] = @CodigoAnalisePerformace_Original
    AND [IndicaRegistroEditavel] = 'S'
    
IF @@ROWCOUNT = 0 AND (@Analise_Original <> @Analise OR @Analise_Original IS NULL)
BEGIN
INSERT INTO {0}.{1}.[AnalisePerformance]
           ([CodigoObjetoAssociado]
           ,[CodigoTipoAssociacao]
           ,[DataAnalisePerformance]
           ,[Analise]
           ,[CodigoUsuarioInclusao]
           ,[DataInclusao]           
           ,[IndicaRegistroEditavel])
     VALUES
           (@CodigoObjetoAssociado
           ,@CodigoTipoAssociacao
           ,@Data
           ,@Analise
           ,@CodigoUsuario
           ,@Data          
           ,'S')

    SET @CodigoAnalisePerformace = @@IDENTITY

IF @CodigoAnalisePerformace_Original IS NULL
 UPDATE {0}.{1}.[StatusReport]
    SET [CodigoAnalisePerformance] = @CodigoAnalisePerformace
  WHERE [CodigoAnalisePerformance] IS NULL
    AND [CodigoObjeto] = @CodigoObjetoAssociado
    AND [CodigoTipoAssociacaoObjeto] = @CodigoTipoAssociacao
    AND [DataPublicacao] IS NULL
ELSE
 UPDATE {0}.{1}.[StatusReport]
    SET [CodigoAnalisePerformance] = @CodigoAnalisePerformace
  WHERE [CodigoAnalisePerformance] = @CodigoAnalisePerformace_Original
    AND [DataPublicacao] IS NULL

END    
    
 UPDATE {0}.{1}.[StatusReport]
    SET [ComentarioFisico] = @ComentarioFisico
       ,[ComentarioFinanceiro] = @ComentarioFinanceiro
       ,[ComentarioRisco] = @ComentarioRisco
       ,[ComentarioQuestao] = @ComentarioQuestao
       ,[ComentarioMeta] = @ComentarioMeta
       ,[ComentarioPlanoAcao] = @ComentarioPlanoAcao
       ,[DataUltimaAlteracao] = @Data
       ,[CodigoUsuarioUltimaAlteracao] = @CodigoUsuario
  WHERE [CodigoStatusReport] = @CodigoStatusReport"
            , bancodb, Ownerdb, codigoStatusReport, comentariosGerais
            , analiseDesempenhoFisico, analiseDesempenhoFinanceiro, analiseRiscos
            , analiseQuestoes, analiseMetas, codigoUsuarioResponsavel, comentarioPlanoAcao);
        classeDados.execSQL(comandoSql, ref registrosAfetados);
    }

    #endregion
}
}