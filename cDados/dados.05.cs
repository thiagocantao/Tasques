using System;
using System.Data;
using System.Text;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region _Portfólio - Visão Corporativa

    public DataSet getEsforcoProjetos(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ISNULL(Sum(rp.TrabalhoLBData), 0) AS TotalTrabalhoPrevisto,
                     ISNULL(Sum(rp.TrabalhoLBTotal), 0) AS TotalTrabalhoPrevistoGeral,
                     ISNULL(Sum(rp.TrabalhoRealTotal), 0) AS TotalTrabalhoReal,
                     ISNULL(Sum(rp.CustoLBData), 0) AS TotalCustoOrcado,
                     ISNULL(Sum(rp.CustoLBTotal), 0) AS TotalCustoOrcadoGeral,
                     ISNULL(Sum(rp.CustoRealTotal), 0) AS TotalCustoReal,
                     ISNULL(Sum(rp.ReceitaLBData), 0) AS TotalReceitaOrcada,
                     ISNULL(Sum(rp.ReceitaLBTotal), 0) AS TotalReceitaOrcadaGeral,
                     ISNULL(Sum(rp.ReceitaRealData), 0) AS TotalReceitaReal
                FROM {0}.{1}.ResumoProjeto AS rp
	                 INNER JOIN {0}.{1}.Projeto AS p ON p.CodigoProjeto = rp.CodigoProjeto
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) F on (f.codigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
               ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCoresBulletsGeral(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                      DECLARE @DesempenhoTrabalho Numeric(18,4),
                              @TrabalhoLBData     Numeric(18,4),
                              @TrabalhoRealData   Numeric(18,4),
                              @UltimoStatusTrabalho SmallInt,
                              @CorTrabalho        Varchar(30)

                      DECLARE @DesempenhoCusto Numeric(18,4),
                              @CustoLBData    Numeric(18,4),
                              @CustoRealData   Numeric(18,4),
                              @UltimoStatusCusto SmallInt,
                              @CorCusto        Varchar(30)

                      DECLARE @DesempenhoReceita Numeric(18,4),
                              @ReceitaLBData    Numeric(18,4),
                              @ReceitaRealData   Numeric(18,4),
                              @UltimoStatusReceita SmallInt,
                              @CorReceita        Varchar(30)

                         
                         SELECT @TrabalhoLBData = Sum(rp.TrabalhoLBData),
                                @TrabalhoRealData = Sum(rp.TrabalhoRealTotal),
                                @CustoLBData = Sum(rp.CustoLBData),
                                @CustoRealData = Sum(rp.CustoRealTotal),
                                @ReceitaLBData = Sum(rp.ReceitaLBData),
                                @ReceitaRealData = Sum(rp.ReceitaRealData)
                           FROM {0}.{1}.ResumoProjeto rp,
                                {0}.{1}.Projeto p
                           INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.codigoProjeto = p.CodigoProjeto)
                           INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                           {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                          WHERE rp.codigoProjeto = p.codigoProjeto
                            {6}/*Somente os projetos em execução*/
                      
                      
                         IF @TrabalhoLBData = 0
                            SET @DesempenhoTrabalho = 0
                         ELSE
                            SET @DesempenhoTrabalho = @TrabalhoRealData / @TrabalhoLBData


                         SELECT TOP 1 @UltimoStatusTrabalho = par_ind.codigotipostatus 
                           FROM {0}.{1}.parametroindicadores par_ind
                          WHERE (@DesempenhoTrabalho*100 >= par_ind.valorinicial
                            AND @DesempenhoTrabalho*100 <= par_ind.valorfinal)
                            AND par_ind.tipoindicador = 'TRA'          

                         SELECT @CorTrabalho = ca.CorApresentacao
                           FROM {0}.{1}.TipoStatusAnalise AS tsa 
                           INNER JOIN {0}.{1}.CorApresentacao AS ca ON ca.CodigoCorApresentacao = tsa.CodigoCorApresentacao
                           WHERE CodigoTipoStatus = @UltimoStatusTrabalho

                     
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

                         SELECT @CorTrabalho AS CorFisico, @CorCusto AS CorFinanceiro, @CorReceita AS CorReceita

                    END", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoBulletsSinalizadores(DataTable dt, DataTable dtCores, string tipoDados,
        string colunaPrevistoData, string colunaPrevistoGeral, string colunaReal, string valorLimite, string urlImage, string subTitulo, int fonte, string urlLink)
    {
        StringBuilder xml = new StringBuilder();
        int i = 0;
        string cor = "", limite = "";
        double previstoData = 0, previstoGeral = 0, realGeral = 0;

        for (i = 0; i < dt.Rows.Count; i++)
        {
            previstoData = previstoData + double.Parse(dt.Rows[i][colunaPrevistoData].ToString());
            previstoGeral = previstoGeral + double.Parse(dt.Rows[i][colunaPrevistoGeral].ToString());
            realGeral = realGeral + double.Parse(dt.Rows[i][colunaReal].ToString());
        }

        if (tipoDados == "Esforço")
        {
            limite = "";
            if (dtCores.Rows[0]["CorFisico"].ToString() == "Verde")
            {
                cor = corSatisfatorio;
            }
            else
            {
                if (dtCores.Rows[0]["CorFisico"].ToString() == "Amarelo")
                {
                    cor = corAtencao;
                }
                else
                {
                    cor = corCritico;
                }
            }
        }
        else
        {
            limite = "upperLimit=\"" + valorLimite + "\"";
            if ((tipoDados == "Custo" || tipoDados == "Despesa") && (dtCores.Rows.Count > 0))
            {
                int codigoEntidadeParam = getInfoSistema("CodigoEntidade") == null ? 1 : int.Parse(getInfoSistema("CodigoEntidade").ToString());
                DataSet dsParam = getParametrosSistema(codigoEntidadeParam, "labelDespesa");

                if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]) && dsParam.Tables[0].Rows[0]["labelDespesa"].ToString() != "")
                    tipoDados = dsParam.Tables[0].Rows[0]["labelDespesa"].ToString();

                if (dtCores.Rows[0]["CorFinanceiro"].ToString() == "Verde")
                {
                    cor = corSatisfatorio;
                }
                else
                {
                    if (dtCores.Rows[0]["CorFinanceiro"].ToString() == "Amarelo")
                    {
                        cor = corAtencao;
                    }
                    else
                    {
                        cor = corCritico;
                    }
                }

            }
            else
            {
                if (dtCores.Rows.Count > 0)
                {
                    if (dtCores.Rows[0]["CorReceita"].ToString() == "Verde")
                    {
                        cor = corSatisfatorio;
                    }
                    else
                    {
                        if (dtCores.Rows[0]["CorReceita"].ToString() == "Amarelo")
                        {
                            cor = corAtencao;
                        }
                        else
                        {
                            cor = corCritico;
                        }
                    }
                }
            }
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

        xml.Append("<chart imageSave=\"1\" manageResize=\"1\" imageSaveURL=\"" + urlImage + "\" palette=\"1\" majorTMNumber=\"3\" minorTMNumber=\"3\" adjustTM=\"0\" caption=\"" + tipoDados + "\" plotFillColor=\"" + cor + "\" animation=\"1\" lowerLimit=\"0\"" +
            " chartTopMargin=\"5\" chartRightMargin=\"30\" valuePadding=\"0\" inThousandSeparator=\".\" chartBottomMargin=\"2\" chartLeftMargin=\"1\" showPlotBorder=\"1\" plotBorderColor='333333' plotBorderThickness='1' colorRangeFillRatio=\"0,10,80,10\" plotBorderAlpha='35'  " +
            " showShadow=\"1\" baseFontSize=\"" + fonte + "\" subcaption=\"" + subTitulo + "\"" +
            " showColorRangeBorder=\"1\" plotFillPercent=\"55\" targetFillPercent=\"65\"  " + usarGradiente + usarBordasArredondadas + exportar + urlLink + " roundRadius=\"0\" " + limite + " formatNumberScale=\"1\" thousandSeparator=\".\" canvasLeftMargin=\"" + (7 * fonte) + "\">");

        xml.Append("<colorRange>");
        xml.Append("<color minValue=\"0\" maxValue=\"" + string.Format("{0:n0}", previstoGeral).Replace(".", "") + "\" code=\"" + corFundoBullets + "\"/>");
        xml.Append("</colorRange>");
        xml.Append("<value>" + string.Format("{0:n0}", realGeral).Replace(".", "") + "</value>");
        xml.Append("<target>" + string.Format("{0:n0}", previstoData).Replace(".", "") + "</target>");
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append("<style name=\"fontTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte + 1) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("<style name=\"fontSubTitulo\" type=\"font\" face=\"Verdana\" size=\"" + (fonte) + "\" color=\"000000\" bold=\"0\"/>");
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append("<apply toObject=\"Caption\" styles=\"fontTitulo\" />");
        xml.Append("<apply toObject=\"SubCaption\" styles=\"fontSubTitulo\" />");
        xml.Append("<apply toObject=\"LIMITVALUES\" styles=\"fontSubTitulo\" />");
        xml.Append("<apply toObject=\"TICKVALUES\" styles=\"fontSubTitulo\" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");

        return xml.ToString();
    }

    public DataSet getQuantidadeProjetosDesempenhoEntidade(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT  (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Azul') AS Excelente,
             (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Verde') AS Satisfatorio,
             (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Amarelo') AS Atencao,
             (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Laranja') AS Finalizando,
             (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Branco') AS Sem_Acompanhamento,
             (SELECT Count(1)
                FROM {0}.{1}.ResumoProjeto as rp  
                     INNER JOIN {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto                             
                     INNER JOIN {0}.{1}.f_GetProjetosUsuario( {2}, {4}, {5} ) F on (F.CodigoProjeto = rp.CodigoProjeto)
                     INNER JOIN {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) 
                     INNER JOIN {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
               WHERE 1 = 1 {6} 
                 AND (rp.CorGeral) = 'Vermelho') AS Critico
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoEntidade(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarLegenda)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();


        int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
        int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

        linkLista = (linkLista == true && podeAcessarLink == true);

        string link = ""; ;
        int i = 0;

        try
        {
            if (linkLista)
                link = @"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=S&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Excelente"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../resumoProjetos.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Satisfatório"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Crítico"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (mostraLaranja == "S")
            {
                if (linkLista)
                    link = @"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Azul=N&Branco=N&Programas=N"" ";

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString(),
                                                                                                               "FF6600",
                                                                                                               link));
            }

            if (linkLista)
                link = @"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=S&Programas=N"" ";

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
            " adjustDiv=\"1\" enablesmartlabels=\"0\" labelDistance=\"1\" showLegend=\"1\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
            " showValues=\"1\" " + usarGradiente + usarBordasArredondadas + exportar + " use3Dlighting=\"0\" legendBorderAlpha=\"0\" showShadow=\"0\" showLabels=\"0\" decimals=\"0\" baseFontSize=\"" + fonte + "\">");
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

    public string getGraficoDesempenhoEntidadeRecurso(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarLegenda)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();


        int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
        int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        bool podeAcessarLink = VerificaPermissaoUsuario(codigoUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, codigoEntidadeUsuarioResponsavel, "NULL", "EN", 0, "NULL", "AcsLstPrj");

        linkLista = (linkLista == true && podeAcessarLink == true);

        string link = ""; ;
        int i = 0;

        try
        {
            if (linkLista)
                link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=S&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Excelente"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Excelente"].ToString(),
                                                                                                           corExcelente,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Satisfatório"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Satisfatorio"].ToString(),
                                                                                                           corSatisfatorio,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Atencao"].ToString(),
                                                                                                           corAtencao,
                                                                                                           link));

            if (linkLista)
                link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Azul=N&Branco=N&Programas=N"" ";

            xmlAux.Append(string.Format(@"<set label=""Crítico"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Critico"].ToString(),
                                                                                                           corCritico,
                                                                                                           link));

            if (mostraLaranja == "S")
            {
                if (linkLista)
                    link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Azul=N&Branco=N&Programas=N"" ";

                xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Finalizando"].ToString(),
                                                                                                               "FF6600",
                                                                                                               link));
            }

            if (linkLista)
                link = @"link=""F-_top-../../_Projetos/resumoProjetosGPWeb.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=S&Programas=N"" ";

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
            " adjustDiv=\"1\" enablesmartlabels=\"0\" labelDistance=\"1\" showLegend=\"" + mostrarLegenda + "\" minimiseWrappingInLegend=\"1\"  legendNumColumns=\"3\" " +
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

    public DataSet getQuantidadeProjetosCategoria(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT c.CodigoCategoria AS Codigo, c.SiglaCategoria AS Descricao,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                   WHERE 1 = 1 {6} 
                       AND p.CodigoCategoria = c.CodigoCategoria AND r.CorGeral = 'Verde') AS Verdes,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                   WHERE 1 = 1 {6} 
                       AND p.CodigoCategoria = c.CodigoCategoria AND r.CorGeral = 'Amarelo') AS Amarelos,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                   WHERE 1 = 1 {6} 
                       AND p.CodigoCategoria = c.CodigoCategoria AND r.CorGeral = 'Vermelho') AS Vermelhos,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                   WHERE 1 = 1 {6} 
                       AND p.CodigoCategoria = c.CodigoCategoria AND r.CorGeral = 'Laranja') AS Laranjas,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                   WHERE 1 = 1 {6} 
                       AND p.CodigoCategoria = c.CodigoCategoria AND r.CorGeral = 'Branco') AS Sem_Acompanhamento
                  FROM {0}.{1}.Categoria c
                 WHERE c.DataExclusao IS NULL
                   AND c.CodigoCategoria IN (SELECT CodigoCategoria 
						                       FROM {0}.{1}.Projeto p INNER JOIN
								                    {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                                    {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3})INNER JOIN 
                                                    {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                              WHERE 1 = 1 {6} )
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getQuantidadeProjetosUnidade(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT u.CodigoUnidadeNegocio AS Codigo, u.SiglaUnidadeNegocio AS Descricao,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6} 
                       AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio AND r.CorGeral = 'Verde') AS Verdes,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6} 
                       AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio AND r.CorGeral = 'Amarelo') AS Amarelos,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6} 
                       AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio AND r.CorGeral = 'Vermelho') AS Vermelhos,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6} 
                       AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio AND r.CorGeral = 'Laranja') AS Laranjas,
                    (SELECT COUNT(1) FROM {0}.{1}.Projeto p INNER JOIN
					                      {0}.{1}.ResumoProjeto r ON r.CodigoProjeto = p.CodigoProjeto INNER JOIN
					                      {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                          {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6} 
                       AND p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio AND r.CorGeral = 'Branco') AS Sem_Acompanhamento
                  FROM {0}.{1}.UnidadeNegocio u
                 WHERE u.CodigoUnidadeNegocio IN (SELECT p.CodigoUnidadeNegocio 
						                       FROM {0}.{1}.Projeto p INNER JOIN
								                    {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = p.CodigoProjeto INNER JOIN 
                                                    {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                          {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                                    WHERE 1 = 1 {6})
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDesempenhoProjetos(DataTable dt, string titulo, int fonte, bool linkLista, int mostrarValores, int mostrarLegenda)
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

            linkLista = (linkLista == true && podeAcessarLink == true);

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
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=N&CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Satisfatórios"" value=""{0}"" color=""{1}"" {2} {3}/>", dt.Rows[i]["Verdes"].ToString()
                                                                                                             , corSatisfatorio
                                                                                                             , link
                                                                                                             , dt.Rows[i]["Verdes"].ToString() == "0" ? "showValue='0'" : ""));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Atenção\" color=\"" + corAtencao + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Azul=N&Branco=N&CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Atenção"" value=""{0}"" color=""{1}"" {2} {3} />", dt.Rows[i]["Amarelos"].ToString()
                                                                                                             , corAtencao
                                                                                                             , link
                                                                                                             , dt.Rows[i]["Amarelos"].ToString() == "0" ? "showValue='0'" : ""));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Críticos\" color=\"" + corCritico + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Azul=N&Branco=N&CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Críticos"" value=""{0}"" color=""{1}"" {2} {3}/>", dt.Rows[i]["Vermelhos"].ToString()
                                                                                                             , corCritico
                                                                                                             , link
                                                                                                             , dt.Rows[i]["Vermelhos"].ToString() == "0" ? "showValue='0'" : ""));
            }

            xmlAux.Append("</dataset>");

            if (mostraLaranja == "S")
            {
                //gera as colunas de projetos em atenção para cada entidade
                xmlAux.Append("<dataset seriesName=\"Finalizando\" color=\"FF6600\">");

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    if (linkLista)
                        link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Azul=N&Branco=N&CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                    xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2} {3}/>", dt.Rows[i]["Laranjas"].ToString()
                                                                                                                 , "FF6600"
                                                                                                                 , link
                                                                                                                 , dt.Rows[i]["Laranjas"].ToString() == "0" ? "showValue='0'" : ""));
                }

                xmlAux.Append("</dataset>");
            }

            xmlAux.Append("<dataset seriesName=\"Sem Informação\" color=\"F3F3F3\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Azul=N&Branco=S&CUN={0}&Programas=N"" ", dt.Rows[i]["Codigo"].ToString());

                xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2} {3}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString()
                                                                                                             , "F3F3F3"
                                                                                                             , link
                                                                                                             , dt.Rows[i]["Sem_Acompanhamento"].ToString() == "0" ? "showValue='0'" : ""));
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
        xml.Append("<chart imageSave=\"1\" caption=\"" + titulo + "\" adjustDiv=\"1\" numVisiblePlot=\"10\"  scrollHeight=\"12\"  showLegend=\"1\"" +
                 " BgColor=\"F7F7F7\" canvasBgColor=\"F7F7F7\" numDivLines=\"3\" showvalues=\"" + mostrarValores + "\" decimals=\"0\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
                 " chartTopMargin=\"3\" chartRightMargin=\"5\" chartBottomMargin=\"2\" chartLeftMargin=\"4\" slantLabels=\"1\" labelDisplay=\"ROTATE\"" +
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\"  decimalPrecision=\"0\" >");
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

    public string getGraficoHistoricoDesempenhoProjetos(DataTable dt, string titulo, int fonte, bool linkLista)
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

            linkLista = (linkLista == true && podeAcessarLink == true);

            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Periodo"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Satisfatórios\"  color=\"" + corSatisfatorio + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=S&Amarelo=N&Vermelho=N&Laranja=N&Programas=N&Branco=N"" ");

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
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=S&Vermelho=N&Laranja=N&Programas=N&Branco=N"" ");

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
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=S&Laranja=N&Programas=N&Branco=N"" ");

                xmlAux.Append(string.Format(@"<set label=""Críticos"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Vermelhos"].ToString()
                                                                                                             , corCritico
                                                                                                             , link));
            }

            xmlAux.Append("</dataset>");

            if (mostraLaranja == "S")
            {
                //gera as colunas de projetos em atenção para cada entidade
                xmlAux.Append("<dataset seriesName=\"Finalizando\" color=\"FF6600\">");

                //percorre todo o DataTable
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    if (linkLista)
                        link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=S&Programas=N&Branco=N"" ");

                    xmlAux.Append(string.Format(@"<set label=""Finalizando"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Laranjas"].ToString()
                                                                                                                 , "FF6600"
                                                                                                                 , link));
                }

                xmlAux.Append("</dataset>");
            }

            //gera as colunas de projetos sem informações para cada entidade
            xmlAux.Append("<dataset seriesName=\"Sem Informação\" color=\"F3F3F3\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (linkLista)
                    link = string.Format(@"link=""F-_top-../resumoProjetos.aspx?Verde=N&Amarelo=N&Vermelho=N&Laranja=N&Programas=N&Branco=S"" ");

                xmlAux.Append(string.Format(@"<set label=""Sem Informação"" value=""{0}"" color=""{1}"" {2}/>", dt.Rows[i]["Sem_Acompanhamento"].ToString()
                                                                                                             , "F3F3F3"
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

    public DataSet getDespesaReceitaCategoria(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT c.CodigoCategoria, c.SiglaCategoria AS DescricaoCategoria,
	                SUM(rp.ReceitaTendenciaTotal) AS Receita, SUM(rp.CustoTendenciaTotal) AS Custo
                  FROM {0}.{1}.Categoria c INNER JOIN
                       {0}.{1}.Projeto p ON p.CodigoCategoria = c.CodigoCategoria INNER JOIN
	                   {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN
	                   {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = rp.CodigoProjeto INNER JOIN 
                       {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                   AND pp.CodigoPortfolio = {3}) INNER JOIN 
                       {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                WHERE c.DataExclusao IS NULL {6} 
                GROUP BY c.CodigoCategoria, c.SiglaCategoria
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDespesaReceitaUnidade(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT u.CodigoUnidadeNegocio, u.SiglaUnidadeNegocio AS DescricaoCategoria,
	                SUM(rp.ReceitaTendenciaTotal) AS Receita, SUM(rp.CustoTendenciaTotal) AS Custo
                  FROM {0}.{1}.UnidadeNegocio u INNER JOIN
					   {0}.{1}.Projeto p ON p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio INNER JOIN
	                   {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto INNER JOIN
	                   {0}.{1}.f_GetProjetosUsuario({2}, {4}, {5}) f ON f.codigoProjeto = rp.CodigoProjeto INNER JOIN 
                       {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                   AND pp.CodigoPortfolio = {3}) INNER JOIN 
                       {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                WHERE 1 = 1 {6} 
                GROUP BY u.CodigoUnidadeNegocio, u.SiglaUnidadeNegocio
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoDespesaReceitaCategorias(DataTable dt, string titulo, int fonte, string urlImage)
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
                xmlAux.Append("<category label=\"" + dt.Rows[i]["DescricaoCategoria"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Despesa\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {

                xmlAux.Append("<set label=\"Despesa\" value=\"" + dt.Rows[i]["Custo"].ToString() + "\"/>");
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Receita\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<set label=\"Receita\" value=\"" + dt.Rows[i]["Receita"].ToString() + "\"/>");
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
        xml.Append("<chart imageSave=\"1\" use3DLighting=\"0\" showShadow=\"0\" imageSaveURL=\"" + urlImage + "\" caption=\"" + titulo + "\" palette=\"1\" inDecimalSeparator=\",\" yAxisNamePadding=\"4\" chartLeftMargin=\"4\"" +
            " legendPadding=\"2\" showBorder=\"0\" baseFontSize=\"" + fonte + "\" " + //numVisiblePlot=\"10\"" +
            " chartRightMargin=\"20\" showLegend=\"1\" scrollHeight=\"12\" BgColor=\"F7F7F7\" slantLabels=\"1\" labelDisplay=\"ROTATE\" " +
            " canvasBgColor=\"F7F7F7\" chartTopMargin=\"10\" numVisiblePlot=\"6\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
            " decimalSeparator=\".\" " + usarGradiente + usarBordasArredondadas + exportar + " showNames=\"1\" adjustDiv=\"1\" showValues=\"0\" decimals=\"2\" chartBottomMargin=\"10\">");

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

    public DataSet getTabelaDesempenhoCategoria(int codigoEntidade, int codigoPortfolio, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT * FROM {0}.{1}.f_GetDesempenhoCategorias({2}, {3}, GetDate())
            ", bancodb, Ownerdb, codigoEntidade, codigoPortfolio, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getNumerosInstituicao(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT 
                        (SELECT count(1) 
                          FROM {0}.{1}.ResumoProjeto comp
                         WHERE comp.CodigoProjeto IN(SELECT f.codigoProjeto FROM {0}.{1}.f_getProjetosUsuario({2}, {4}, {5}) f INNER JOIN 
                                                                                 {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = f.CodigoProjeto 
                                                                                    AND pp.CodigoPortfolio = {3}) INNER JOIN 
                                                                                 {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                                                    AND s.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
                                                                                 {0}.{1}.Projeto p ON p.CodigoProjeto = f.CodigoProjeto
                                                                           WHERE 1 = 1 {6})) AS qtdProjetos,                                 
                        isNull(Sum(CustoLBData)     , 0) AS CustoOrcadoData,
                        isNull(Sum(CustoLBTotal)    , 0) AS CustoOrcadoTotal,
                        isNull(Sum(CustoRealData)   , 0) AS CustoRealData,
                        isNull(Sum(CustoRealTotal)  , 0) AS CustoRealTotal,
                        isNull(Sum(TrabalhoLBData)  , 0) AS TrabalhoOrcadoData,
                        isNull(Sum(TrabalhoLBTotal) , 0) AS TrabalhoOrcadoTotal,
                        isNull(Sum(TrabalhoRealData), 0) AS TrabalhoRealData,
                        isNull(Sum(TrabalhoRealTotal), 0) AS TrabalhoRealTotal,
                        isNull(Sum(ReceitaLBData)   , 0) AS ReceitaOrcadoData,
                        isNull(Sum(ReceitaLBTotal)  , 0) AS ReceitaOrcadoTotal,
                        isNull(Sum(ReceitaRealData) , 0) AS ReceitaRealData,
                        isNull(Sum(ReceitaRealTotal), 0) AS ReceitaRealTotal                                
                FROM {0}.{1}.ResumoProjeto rp INNER JOIN 
                     {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = rp.CodigoProjeto 
                                                     AND pp.CodigoPortfolio = {3}) INNER JOIN 
                     {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
                     {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto
               WHERE rp.CodigoProjeto in(SELECT codigoProjeto FROM {0}.{1}.f_getProjetosUsuario({2}, {4}, {5})) {6}
            ", bancodb, Ownerdb, codigoUsuario, codigoPortfolio, codigoEntidade, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGanttProjetos(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
               @"BEGIN
                      DECLARE @tblRetorno TABLE
                       (Codigo Int,
                        Descricao Varchar(255),
                        Inicio DateTime,
                        Termino DateTime,
                        InicioReal DateTime,
                        TerminoReal DateTime,
                        Cor Varchar(20),
                        Concluido Decimal(5,2),
                        CodigoPai Int,
                        CodigoCategoria Int,
                        Duracao Decimal(10,2),
                        DuracaoReal Decimal(10,2))
                        
                        /* Insere os projetos */
                        INSERT INTO @tblRetorno
                         (Codigo,
                          Descricao,
                          Inicio,
                          Termino,
                          InicioReal,
                          TerminoReal,
                          Cor,
                          Concluido,
                          CodigoPai,
                          CodigoCategoria,
                          Duracao,
                          DuracaoReal)
	                    SELECT f.CodigoProjeto AS Codigo, 
		                       f.NomeProjeto AS Descricao, 
		                       f.Inicio, 
		                       f.Termino,
                               p.InicioReal,
                               p.TerminoReal, 
		                       f.Cor, 
		                       p.PercentualRealizacao * 100 AS Concluido,
		                       proj.CodigoCategoria * -1 AS CodigoPai,
		                       proj.CodigoCategoria,
		                       p.DuracaoTendencia,
		                       p.DuracaoReal
	                      FROM {0}.{1}.f_GetGanttProjetos({2}, -1, {3}) f INNER JOIN
		                       {0}.{1}.ResumoProjeto p ON p.CodigoProjeto = f.CodigoProjeto INNER JOIN 
		                       {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
								                       AND pp.CodigoPortfolio = {3}) INNER JOIN 
		                       {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
						                       AND s.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
		                       {0}.{1}.f_getProjetosUsuario({4}, {2}, {5}) fu ON fu.CodigoProjeto = p.CodigoProjeto INNER JOIN
		                       {0}.{1}.Projeto AS proj ON (proj.CodigoProjeto = p.CodigoProjeto)
	                     WHERE 1 = 1 {6}
                    	 
	                     /* Insere as categorias */
	                     INSERT INTO @tblRetorno
	                     (Codigo,
                          Descricao,
                          Inicio,
                          Termino,
                          InicioReal,
                          TerminoReal,
                          Cor,
                          Duracao,
                          DuracaoReal)
                          SELECT c.CodigoCategoria * -1,
                                 c.DescricaoCategoria,
                                 MIN(t.Inicio),
                                 MAX(t.Termino),
                                 MIN(t.InicioReal),
                                 MAX(t.TerminoReal),
                                 'Cinza',
                                 SUM(Duracao),
                                 SUM(DuracaoReal) 
                            FROM {0}.{1}.Categoria AS c INNER JOIN
                                 @tblRetorno AS t ON (t.CodigoCategoria = c.CodigoCategoria)
                          GROUP BY c.CodigoCategoria * -1,
                                   c.DescricaoCategoria   
                                   
                        SELECT t.Codigo,
                               t.Descricao,
                               t.Inicio,
                               t.Termino,
                               t.InicioReal,
                               t.TerminoReal,
                               t.Cor,
                               CASE WHEN t.CodigoCategoria IS NULL AND t.Duracao <> 0
                                         THEN Convert(SmallInt, t.DuracaoReal / t.Duracao * 100)
                                    WHEN t.CodigoCategoria IS NULL AND t.Duracao = 0
                                         THEN 0
                                         ELSE t.Concluido END AS Concluido,
                               t.CodigoPai,
                               t.Duracao,
                               t.DuracaoReal,
                               'N' AS SemCronograma
                          FROM @tblRetorno AS t 
                         ORDER BY t.CodigoPai, t.Inicio, t.Termino           
                    END",
               bancodb, Ownerdb, codigoEntidade, codigoPortfolio, codigoUsuario, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGanttPortfolioUnidade(int codigoUsuario, int codigoEntidade, int codigoPortfolio, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(
               @"BEGIN
                      DECLARE @tblRetorno TABLE
                               (Codigo Int,
                                Descricao Varchar(255),
                                Inicio DateTime,
                                Termino DateTime,
                                InicioReal DateTime,
                                TerminoReal DateTime,
                                Cor Varchar(20),
                                Concluido Decimal(5,2),
                                CodigoPai Int,
                                CodigoUnidade Int,
                                Duracao Decimal(10,2),
                                DuracaoReal Decimal(10,2))
                        
                        /* Insere os projetos */
                        INSERT INTO @tblRetorno
                                     (Codigo,
                                      Descricao,
                                      Inicio,
                                      Termino,
                                      InicioReal,
                                      TerminoReal,
                                      Cor,
                                      Concluido,
                                      CodigoPai,
                                      CodigoUnidade,
                                      Duracao,
                                      DuracaoReal)
	                                SELECT f.CodigoProjeto AS Codigo, 
		                                   f.NomeProjeto AS Descricao, 
		                                   f.Inicio, 
		                                   f.Termino, 
		                                   p.InicioReal, 
		                                   p.TerminoReal, 
		                                   f.Cor, 
		                                   p.PercentualRealizacao * 100 AS Concluido,
		                                   proj.CodigoUnidadeNegocio * -1 AS CodigoPai,
		                                   proj.CodigoUnidadeNegocio,
		                                   p.DuracaoTendencia,
		                                   p.DuracaoReal
	                      FROM {0}.{1}.f_GetGanttProjetos({2}, -1, {3}) f INNER JOIN
		                       {0}.{1}.ResumoProjeto p ON p.CodigoProjeto = f.CodigoProjeto INNER JOIN 
		                       {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
								                       AND pp.CodigoPortfolio = {3}) INNER JOIN 
		                       {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
						                       AND s.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
		                       {0}.{1}.f_getProjetosUsuario({4}, {2}, {5}) fu ON fu.CodigoProjeto = p.CodigoProjeto INNER JOIN
		                       {0}.{1}.Projeto AS proj ON (proj.CodigoProjeto = p.CodigoProjeto)
	                     WHERE 1 = 1 {6}
                    	 
	                     /* Insere as categorias */
	                     INSERT INTO @tblRetorno
	                     (Codigo,
                          Descricao,
                          Inicio,
                          Termino,
                          InicioReal,
                          TerminoReal,
                          Cor,
                          Duracao,
                          DuracaoReal)
                          SELECT u.CodigoUnidadeNegocio * -1,
                                 u.NomeUnidadeNegocio,
                                 MIN(t.Inicio),
                                 MAX(t.Termino),
                                 MIN(t.InicioReal),
                                 MAX(t.TerminoReal),
                                 'Cinza',
                                 SUM(Duracao),
                                 SUM(DuracaoReal) 
                            FROM {0}.{1}.UnidadeNegocio AS u INNER JOIN
                                 @tblRetorno AS t ON (t.CodigoUnidade = u.CodigoUnidadeNegocio)
                          GROUP BY u.CodigoUnidadeNegocio * -1,
                                   u.NomeUnidadeNegocio   
                                   
                        SELECT t.Codigo,
                               t.Descricao,
                               t.Inicio,
                               t.Termino,
                               t.InicioReal,
                               t.TerminoReal,
                               t.Cor,
                               CASE WHEN t.CodigoUnidade IS NULL AND t.Duracao <> 0
                                         THEN Convert(SmallInt, t.DuracaoReal / t.Duracao * 100)
                                    WHEN t.CodigoUnidade IS NULL AND t.Duracao = 0
                                         THEN 0
                                         ELSE t.Concluido END AS Concluido,
                               t.CodigoPai,
                               t.Duracao,
                               t.DuracaoReal,
                               'N' AS SemCronograma
                          FROM @tblRetorno AS t 
                         ORDER BY t.CodigoPai, t.Inicio, t.Termino           
                    END",
               bancodb, Ownerdb, codigoEntidade, codigoPortfolio, codigoUsuario, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGanttProjetosUnidade(int codigoUsuario, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
               @"BEGIN
                      DECLARE @tblRetorno TABLE
                               (Codigo Int,
                                Descricao Varchar(255),
                                Inicio DateTime,
                                Termino DateTime,
                                Cor Varchar(20),
                                Concluido Decimal(5,2),
                                CodigoPai Int,
                                CodigoUnidade Int,
                                Sumaria Char(1),
                                Nivel int)
                        
                        /* Insere os projetos */
                        INSERT INTO @tblRetorno
                                     (Codigo,
                                      Descricao,
                                      Inicio,
                                      Termino,
                                      Cor,
                                      Concluido,
                                      CodigoPai,
                                      CodigoUnidade,
                                      Sumaria, 
                                      Nivel)
	                                SELECT p.CodigoProjeto AS Codigo, 
		                                   prj.NomeProjeto AS Descricao, 
		                                   p.InicioReprogramado AS Inicio, 
		                                   p.TerminoReprogramado AS Termino, 
		                                   {0}.{1}.f_GetCorFisico(p.CodigoProjeto) AS Cor, 
		                                   p.PercentualRealizacao * 100 AS Concluido,
		                                   prj.CodigoUnidadeNegocio * -1 AS CodigoPai,
		                                   prj.CodigoUnidadeNegocio, '0', 3
	                      FROM {0}.{1}.ResumoProjeto p INNER JOIN 
		                       {0}.{1}.Projeto prj ON (prj.CodigoProjeto = p.CodigoProjeto) INNER JOIN 
		                       {0}.{1}.Status AS s ON (s.CodigoStatus = prj.CodigoStatusProjeto
						                       AND s.IndicaAcompanhamentoExecucao = 'S') 
	                     WHERE prj.DataExclusao IS NULL
						   AND prj.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ')
                           AND prj.CodigoStatusProjeto <> 4
                           AND prj.CodigoStatusProjeto <> 6
                           {4}
                    	 
	                     /* Insere as unidades */
	                     INSERT INTO @tblRetorno
	                     (Codigo,
                          Descricao,
                          Inicio,
                          Termino,
                          Cor,
                          CodigoPai,
                          Sumaria, 
                          Nivel)
                          SELECT u.CodigoUnidadeNegocio * -1,
                                 u.SiglaUnidadeNegocio,
                                 MIN(t.Inicio),
                                 MAX(t.Termino),
                                 'Cinza',
                                 u.CodigoUnidadeNegocioSuperior * -1, '1', 2
                            FROM {0}.{1}.UnidadeNegocio AS u INNER JOIN
                                 @tblRetorno AS t ON (t.CodigoUnidade = u.CodigoUnidadeNegocio)
                           WHERE CodigoEntidade = {2}                                   
                          GROUP BY u.CodigoUnidadeNegocio * -1,
                                   u.SiglaUnidadeNegocio, u.CodigoUnidadeNegocioSuperior    
                         
                        DECLARE @CodigoUnidade Int,
            		        @CodigoUnidadeSuperior Int,
            		        @CodigoUnidadeAux Int
            		                                               	
            		/* Cursor para percorrer as unidades de negócio e trazer seus respectivos pais */                                       	                                  
		            DECLARE cCursor CURSOR LOCAL FOR
		            SELECT DISTINCT Codigo * -1
		              FROM @tblRetorno
		             WHERE Codigo < 0
		            
		            OPEN cCursor
		            
		            FETCH NEXT FROM cCursor INTO @CodigoUnidade
		            
                    WHILE @@FETCH_STATUS = 0
		              BEGIN	

		            SET @CodigoUnidadeAux = @CodigoUnidade
		            SET @CodigoUnidadeSuperior = null
		            		            	            
	                SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
	                  FROM {0}.{1}.UnidadeNegocio
	                 WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
	                   AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!
	            	                                
		            
		                WHILE @CodigoUnidadeSuperior IS NOT NULL
		                  BEGIN	  
		                    	                             
		                    INSERT INTO @tblRetorno (Codigo,
													  Descricao,
													  Inicio,
													  Termino,
													  Cor,
													  CodigoPai,
                                                      Sumaria, 
                                                      Nivel)
								 SELECT  un.CodigoUnidadeNegocio * -1,
										 un.SiglaUnidadeNegocio,
										 (SELECT MIN(t.Inicio) FROM @tblRetorno t WHERE (un.CodigoUnidadeNegocio * -1) = t.CodigoPai),
										 (SELECT MAX(t.Termino) FROM @tblRetorno t WHERE (un.CodigoUnidadeNegocio * -1) = t.CodigoPai),
										 'Cinza',
										 un.CodigoUnidadeNegocioSuperior * -1, '1', 1
								  FROM {0}.{1}.UnidadeNegocio AS un  LEFT JOIN
									   {0}.{1}.Usuario AS g ON (g.CodigoUsuario = un.CodigoUsuarioGerente) 
								 WHERE un.CodigoUnidadeNegocio = @CodigoUnidadeSuperior
								   AND (un.CodigoUnidadeNegocio * -1) NOT IN (SELECT Codigo FROM @tblRetorno)
								   AND un.CodigoEntidade = {2} --> Parâmetro!!!!
								   
								 SET @CodigoUnidadeAux = @CodigoUnidadeSuperior
								 
								 SET @CodigoUnidadeSuperior = null
								   
								 SELECT @CodigoUnidadeSuperior = CodigoUnidadeNegocioSuperior
		                           FROM {0}.{1}.UnidadeNegocio
		                          WHERE CodigoUnidadeNegocio = @CodigoUnidadeAux
		                            AND CodigoEntidade = {2} -- Parâmetro CodigoEntidade!!!!  
		                            
		                             
		                      END  
		                 
		                FETCH NEXT FROM cCursor INTO @CodigoUnidade
		              END
		            
		            CLOSE cCursor
		            DEALLOCATE cCursor   
                                
                    UPDATE @tblRetorno
                       SET CodigoPai = Null,
                           Nivel = 0
                     WHERE Not EXISTS (SELECT 1
                                         FROM {0}.{1}.UnidadeNegocio
                                        WHERE CodigoUnidadeNegocio = (CodigoPai * -1)
                                          AND CodigoEntidade = {2})
                                                        
                        SELECT t.Codigo,
                               t.Descricao,
                               t.Inicio,
                               t.Termino,
                               t.Cor AS Status,
                               t.CodigoPai AS CodigoSuperior,
                               t.Concluido,
                               CASE WHEN t.Inicio IS NULL OR t.Termino IS NULL THEN 'S' ELSE 'N' END AS SemCronograma, Sumaria
                          FROM @tblRetorno AS t 
                         ORDER BY t.Nivel, t.CodigoPai, t.Inicio, t.Termino           
                    END",
               bancodb, Ownerdb, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDatasGanttProjetos(int codigoEntidade, int codigoPortfolio, string where)
    {
        string comandoSQL = string.Format(
               @"SELECT MIN(f.Inicio) AS Inicio, MAX(f.Termino) AS Termino, DATEDIFF(MONTH, MIN(f.Inicio), MAX(f.Termino)) AS Meses
                   FROM {0}.{1}.f_GetGanttProjetos({2}, -1, {3}) f INNER JOIN
                        {0}.{1}.Projeto p ON p.CodigoProjeto = f.CodigoProjeto INNER JOIN 
                        {0}.{1}.PortfolioProjeto pp ON (pp.CodigoProjeto = p.CodigoProjeto 
                                                                AND pp.CodigoPortfolio = {3}) INNER JOIN 
                        {0}.{1}.Status AS s ON (s.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                                    AND s.IndicaAcompanhamentoPortfolio = 'S')
                 WHERE 1 = 1 {4} ",
               bancodb, Ownerdb, codigoEntidade, codigoPortfolio, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAlertasCronograma(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT CodigoAlerta, DescricaoAlerta, u.NomeUsuario AS UsuarioInclusao
			            ,DiasAntecedenciaInicio1, DiasIntervaloRecorrenciaInicio2
			            ,DiasAntecedenciaInicio2, DiasIntervaloRecorrenciaInicio3
			            ,DiasIntervaloRecorrenciaTermino, DiasIntervaloRecorrenciaAtraso
			            ,MensagemAlertaInicio1, MensagemAlertaInicio2, MensagemAlertaInicio3
			            ,MensagemAlertaTermino, MensagemAlertaAtraso
			            ,IndicaAlertaInicio1, IndicaAlertaInicio2, IndicaAlertaInicio3
			            ,IndicaAlertaTermino, IndicaAlertaAtraso
              FROM {0}.{1}.Alerta a INNER JOIN
			       {0}.{1}.Usuario u ON u.CodigoUsuario = a.CodigoUsuarioInclusao
             WHERE a.DataExclusao IS NULL
               AND a.CodigoProjeto = {2}
               {3}
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiAlertaCronograma(int codigoProjeto, int codigoUsuario, string nomeRegra, string diasAntecedenciaInicio1
        , string diasIntervaloRecorrenciaInicio2, string diasAntecedenciaInicio2, string diasIntervaloRecorrenciaInicio3
        , string diasIntervaloRecorrenciaTermino, string diasIntervaloRecorrenciaAtraso, string mensagemAlertaInicio1
        , string mensagemAlertaInicio2, string mensagemAlertaInicio3, string mensagemAlertaTermino, string mensagemAlertaAtraso
        , string indicaAlertaInicio1, string indicaAlertaInicio2, string indicaAlertaInicio3, string indicaAlertaTermino
        , string indicaAlertaAtraso, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                       INSERT INTO {0}.{1}.Alerta(DescricaoAlerta, DiasAntecedenciaInicio1, DiasIntervaloRecorrenciaInicio2
			                    ,DiasAntecedenciaInicio2, DiasIntervaloRecorrenciaInicio3, DiasIntervaloRecorrenciaTermino
			                    ,DiasIntervaloRecorrenciaAtraso,MensagemAlertaInicio1, MensagemAlertaInicio2, MensagemAlertaInicio3
			                    ,MensagemAlertaTermino, MensagemAlertaAtraso, IndicaAlertaInicio1, IndicaAlertaInicio2
			                    ,IndicaAlertaInicio3, IndicaAlertaTermino, IndicaAlertaAtraso, DataInclusao, CodigoUsuarioInclusao, CodigoProjeto)
                      VALUES('{2}', {3}, {4}, {5}, {6}, {7}, {8}, '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}'
				                    ,GETDATE(), {19}, {20})
                        ", bancodb, Ownerdb
                         , nomeRegra.Replace("'", "''")
                         , diasAntecedenciaInicio1, diasIntervaloRecorrenciaInicio2
                         , diasAntecedenciaInicio2, diasIntervaloRecorrenciaInicio3, diasIntervaloRecorrenciaTermino
                         , diasIntervaloRecorrenciaAtraso, mensagemAlertaInicio1.Replace("'", "''"), mensagemAlertaInicio2.Replace("'", "''")
                         , mensagemAlertaInicio3.Replace("'", "''"), mensagemAlertaTermino.Replace("'", "''"), mensagemAlertaAtraso.Replace("'", "''")
                         , indicaAlertaInicio1, indicaAlertaInicio2, indicaAlertaInicio3, indicaAlertaTermino, indicaAlertaAtraso, codigoUsuario, codigoProjeto);

            execSQL(comandoSQL, ref registrosAfetados);
            msg = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaAlertaCronograma(int codigoAlerta, int codigoUsuario, string nomeRegra, string diasAntecedenciaInicio1
        , string diasIntervaloRecorrenciaInicio2, string diasAntecedenciaInicio2, string diasIntervaloRecorrenciaInicio3
        , string diasIntervaloRecorrenciaTermino, string diasIntervaloRecorrenciaAtraso, string mensagemAlertaInicio1
        , string mensagemAlertaInicio2, string mensagemAlertaInicio3, string mensagemAlertaTermino, string mensagemAlertaAtraso
        , string indicaAlertaInicio1, string indicaAlertaInicio2, string indicaAlertaInicio3, string indicaAlertaTermino
        , string indicaAlertaAtraso, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                       UPDATE {0}.{1}.Alerta SET DescricaoAlerta = '{2}'
                                               , DiasAntecedenciaInicio1 = {3}
                                               , DiasIntervaloRecorrenciaInicio2 = {4}
			                                   , DiasAntecedenciaInicio2 = {5}
                                               , DiasIntervaloRecorrenciaInicio3 = {6}
                                               , DiasIntervaloRecorrenciaTermino = {7}
			                                   , DiasIntervaloRecorrenciaAtraso = {8}
                                               , MensagemAlertaInicio1 = '{9}'
                                               , MensagemAlertaInicio2 = '{10}'
                                               , MensagemAlertaInicio3 = '{11}'
			                                   , MensagemAlertaTermino = '{12}'
                                               , MensagemAlertaAtraso = '{13}'
                                               , IndicaAlertaInicio1 = '{14}'
                                               , IndicaAlertaInicio2 = '{15}'
			                                   , IndicaAlertaInicio3 = '{16}'
                                               , IndicaAlertaTermino = '{17}'
                                               , IndicaAlertaAtraso = '{18}'
                                               , DataUltimaAlteracao = GetDate()
                                               , CodigoUsuarioUltimaAlteracao = {19}
                      WHERE CodigoAlerta = {20}
                        ", bancodb, Ownerdb
                         , nomeRegra.Replace("'", "''")
                         , diasAntecedenciaInicio1, diasIntervaloRecorrenciaInicio2
                         , diasAntecedenciaInicio2, diasIntervaloRecorrenciaInicio3, diasIntervaloRecorrenciaTermino
                         , diasIntervaloRecorrenciaAtraso, mensagemAlertaInicio1.Replace("'", "''"), mensagemAlertaInicio2.Replace("'", "''")
                         , mensagemAlertaInicio3.Replace("'", "''"), mensagemAlertaTermino.Replace("'", "''"), mensagemAlertaAtraso.Replace("'", "''")
                         , indicaAlertaInicio1, indicaAlertaInicio2, indicaAlertaInicio3, indicaAlertaTermino, indicaAlertaAtraso, codigoUsuario, codigoAlerta);

            execSQL(comandoSQL, ref registrosAfetados);
            msg = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool excluiAlertaCronograma(int codigoAlerta, int codigoUsuario, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                       UPDATE {0}.{1}.Alerta SET DataExclusao = GetDate()
                                               , CodigoUsuarioExclusao = {2}
                      WHERE CodigoAlerta = {3}
                        ", bancodb, Ownerdb
                         , codigoUsuario, codigoAlerta);

            execSQL(comandoSQL, ref registrosAfetados);
            msg = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getTarefasAlertasCronograma(int codigoAlerta, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                @"SELECT at.CodigoAlertaTarefa, tcp.NomeTarefa
                    FROM {0}.{1}.AlertaTarefa at INNER JOIN
			             {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = at.CodigoCronogramaProjeto
													        AND cp.CodigoProjeto = {3}) INNER JOIN
			             {0}.{1}.TarefaCronogramaProjeto tcp ON (tcp.CodigoCronogramaProjeto = cp.CodigoCronogramaProjeto
													        AND tcp.CodigoTarefa = at.CodigoTarefa)
                    WHERE tcp.DataExclusao IS NULL
	                  AND tcp.PercentualFisicoConcluido < 100
	                  AND at.CodigoAlerta = {2}
                      {4}
                    ORDER BY tcp.NomeTarefa
               ", bancodb, Ownerdb, codigoAlerta, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getAlertaTarefa(int codigoAlertaTarefa, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT at.CodigoTarefa, CodigoAlerta
                      FROM {0}.{1}.AlertaTarefa at
                     WHERE at.CodigoAlertaTarefa = {2}
                       {3}
               ", bancodb, Ownerdb, codigoAlertaTarefa, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiTarefaAlerta(int codigoAlerta, int codigoTarefa, int codigoProjeto, string[] listaDestinatario, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            string insertUsuarios = "";

            foreach (string codigoUsuario in listaDestinatario)
            {
                if (codigoUsuario.Trim() != "")
                {
                    insertUsuarios += string.Format(@"INSERT INTO {0}.{1}.AlertaTarefaDestinatario(CodigoAlertaTarefa, CodigoUsuarioDestinatario)
                                                                                       VALUES(@CodigoAlertaTarefa, {2})
                                                 ", bancodb, Ownerdb, codigoUsuario);
                }
            }

            comandoSQL = geraBlocoBeginTran() + "  " + string.Format(@"
                    
                        DECLARE @CodigoCronogramaProjeto VarChar(64),
                                @CodigoAlertaTarefa int

                        SELECT @CodigoCronogramaProjeto = CodigoCronogramaProjeto 
                          FROM CronogramaProjeto
                         WHERE CodigoProjeto = {2}

                       INSERT INTO {0}.{1}.AlertaTarefa(CodigoAlerta, CodigoCronogramaProjeto, CodigoTarefa)
                            VALUES({3}, @CodigoCronogramaProjeto, {4})
                        
                       SET @CodigoAlertaTarefa = SCOPE_IDENTITY();  

                       {5}              

                    
                        ", bancodb, Ownerdb
                         , codigoProjeto
                         , codigoAlerta
                         , codigoTarefa
                         , insertUsuarios);

            comandoSQL += " " + geraBlocoEndTran();
            execSQL(comandoSQL, ref registrosAfetados);
            msg = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTarefaAlerta(int codigoAlertaTarefa, int codigoTarefa, string[] listaDestinatario, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            string insertUsuarios = "";

            foreach (string codigoUsuario in listaDestinatario)
            {
                if (codigoUsuario.Trim() != "")
                {
                    insertUsuarios += string.Format(@"INSERT INTO {0}.{1}.AlertaTarefaDestinatario(CodigoAlertaTarefa, CodigoUsuarioDestinatario)
                                                                                       VALUES(@CodigoAlertaTarefa, {2})
                                                 ", bancodb, Ownerdb, codigoUsuario);
                }
            }
            comandoSQL += " " + geraBlocoBeginTran();
            comandoSQL += string.Format(@"
                    
                        DECLARE @CodigoAlertaTarefa int

                        SET @CodigoAlertaTarefa = {2};  

                       UPDATE {0}.{1}.AlertaTarefa SET CodigoTarefa = {3}
                        WHERE CodigoAlertaTarefa = @CodigoAlertaTarefa
                       
                       DELETE FROM {0}.{1}.AlertaTarefaDestinatario
                        WHERE CodigoAlertaTarefa = @CodigoAlertaTarefa

                       {4}              

                     
                        ", bancodb, Ownerdb
                         , codigoAlertaTarefa
                         , codigoTarefa
                         , insertUsuarios);
            comandoSQL += " " + geraBlocoEndTran();
            execSQL(comandoSQL, ref registrosAfetados);
            msg = "";
            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool excluiTarefaAlerta(int codigoAlertaTarefa, ref string msg)
    {
        bool retorno = false;
        int registrosAfetados = 0;
        try
        {
            comandoSQL = geraBlocoBeginTran() + " " + string.Format(@"
                    
                        DECLARE @CodigoAlertaTarefa int

                        SET @CodigoAlertaTarefa = {2};  
                       
                       DELETE FROM {0}.{1}.AlertaTarefaDestinatario
                        WHERE CodigoAlertaTarefa = @CodigoAlertaTarefa

                       DELETE FROM {0}.{1}.AlertaTarefa
                        WHERE CodigoAlertaTarefa = @CodigoAlertaTarefa          

                     
                        ", bancodb, Ownerdb
                         , codigoAlertaTarefa);
            comandoSQL += " " + geraBlocoEndTran();
            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            msg = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getUsuariosDisponiveisAlerta(int codigoAlertaTarefa, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT Codigo, Descricao
                      FROM {0}.{1}.f_GetInteressadosProjeto({2})
                     WHERE Codigo NOT IN(SELECT CodigoUsuarioDestinatario 
									   FROM {0}.{1}.AlertaTarefaDestinatario
									  WHERE CodigoAlertaTarefa = {3})
                      {4}
                     ORDER BY Descricao
               ", bancodb, Ownerdb, codigoProjeto, codigoAlertaTarefa, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUsuariosAssociadosAlerta(int codigoAlertaTarefa, int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
                  @"SELECT Codigo, Descricao
                      FROM {0}.{1}.f_GetInteressadosProjeto({2})
                     WHERE Codigo IN(SELECT CodigoUsuarioDestinatario 
									   FROM {0}.{1}.AlertaTarefaDestinatario
									  WHERE CodigoAlertaTarefa = {3})
                      {4}
                     ORDER BY Descricao
               ", bancodb, Ownerdb, codigoProjeto, codigoAlertaTarefa, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getTarefasGantt(int codigoProjeto, int codigoRecurso, string where)
    {
        if (codigoRecurso != -1)
        {
            where += string.Format(@" AND CodigoTarefa IN (SELECT a.CodigoTarefa 
                                                               FROM {0}.{1}.vi_Atribuicao a
                                                              WHERE a.CodigoProjeto = {2}
                                                                AND a.CodigoRecurso = @CodigoRecurso)", bancodb, Ownerdb, codigoProjeto);
        }

        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoProjeto int,
                            @CodigoRecurso VarChar(64)

                    SELECT @CodigoRecurso = ResourceUID 
                      FROM {0}.{1}.Usuario 
                     WHERE CodigoUsuario = {4}          	
	                SET @CodigoProjeto = {2}
                	
                    SELECT CodigoTarefa, NomeTarefa, CASE WHEN TarefaSuperior = CodigoTarefa THEN null ELSE TarefaSuperior END AS TarefaSuperior, 
						   PercentualRealTarefa AS Concluido, Inicio, Termino, EstruturaHierarquica, IndicaMarco, IndicaTarefaSumario, CodigoProjeto, Nivel
                      FROM {0}.{1}.vi_Tarefa 
                     WHERE CodigoProjeto = @CodigoProjeto 
                       {3}
                    ORDER BY EstruturaHierarquica
                END
               ", bancodb, Ownerdb, codigoProjeto, where, codigoRecurso);
        return getDataSet(comandoSQL);
    }

    public DataSet getCronogramaGantt(int codigoProjeto, string codigoRecurso, int versaoLB, bool fazInner, bool somenteAtrasadas, bool somenteMarcos, int? percentualConcluido, DateTime? dataFiltro)
    {
        string replanejamentoTasques = "N";
        string permiteUsoPesoManual = "N";

        /*
         [ValorPesoTarefaLB], título "Peso LB" com 2 casas decimais;
         [PercentualPesoTarefa], título "% Peso" com 2 casas decimais.
         */

        string apareceColuna_ValorPesoTarefaLB = ", NULL as ValorPesoTarefaLB ";
        string apareceColuna_PercentualPesoTarefa = ", NULL as PercentualPesoTarefa ";

        DataSet dsParam = getParametrosSistema(int.Parse(getInfoSistema("CodigoEntidade").ToString()), "TASQUES_ReplanejarCronograma", "TASQUES_PermiteUsoPesoManual");

        if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
        {
            replanejamentoTasques = dsParam.Tables[0].Rows[0]["TASQUES_ReplanejarCronograma"].ToString();
            permiteUsoPesoManual = dsParam.Tables[0].Rows[0]["TASQUES_PermiteUsoPesoManual"].ToString();
        }

        if (permiteUsoPesoManual.Trim().ToUpper() == "S")
        {
            apareceColuna_ValorPesoTarefaLB = ", f.ValorPesoTarefaLB";
            apareceColuna_PercentualPesoTarefa = ", f.PercentualPesoTarefa";
        }


        //bool usaVersaoReplanejamento = false;

        //if (replanejamentoTasques == "S")
        //{
        //    string comandoSqlLB = string.Format(
        //         @"BEGIN	               
        //            DECLARE @CodigoProjeto INT

	       //         SET @CodigoProjeto = {2}
	                    
	       //         SELECT ModeloLinhaBase, NumeroVersao
		      //       FROM {0}.{1}.f_crono_GetVersoesLBProjeto(@CodigoProjeto) vlb
		      //      WHERE VersaoLinhaBase = {3}
        //        END
        //       ", bancodb, Ownerdb, codigoProjeto, versaoLB);

        //    DataSet dsLB = getDataSet(comandoSqlLB);

        //    if (DataSetOk(dsLB) && DataTableOk(dsLB.Tables[0]))
        //    {
        //        usaVersaoReplanejamento = dsLB.Tables[0].Rows[0]["ModeloLinhaBase"].ToString().Trim() == "2";

        //        if (!usaVersaoReplanejamento && dsLB.Tables[0].Rows[0]["NumeroVersao"].ToString() == "-1")
        //            versaoLB = -1;
        //    }
        //}

        //if (usaVersaoReplanejamento)
        //{
        //    string comandoSQL = string.Format(
        //    @"BEGIN 
        //        DECLARE @in_CodigoProjeto as int
        //        DECLARE @in_VersaoLinhaBase as smallint
        //        DECLARE @in_CodigoRecurso as int
        //        DECLARE @in_SoAtrasadas as char(1)
        //        DECLARE @in_SoMarcos as  char(1)
        //        DECLARE @in_PercentualConcluido  int
        //        DECLARE @in_DataFiltro  datetime

        //        SET @in_CodigoProjeto = {2}
        //        SET @in_VersaoLinhaBase = {4}
        //        SET @in_CodigoRecurso =  {3}
        //        SET @in_SoAtrasadas = '{5}'
        //        SET @in_SoMarcos = '{6}'
        //        SET @in_PercentualConcluido = {7}
        //        SET @in_DataFiltro = {8} 
  
        //        SELECT 
	       //       f.EstruturaHierarquica, 
			     // f.CodigoTarefa, 
			     // f.NomeTarefa, 
			     // f.Concluido, 
			     // f.Trabalho, 
			     // f.Custo, 
        //          f.Inicio, 
        //          f.Termino,
        //          f.InicioLB AS InicioLB,
        //          f.TerminoLB AS TerminoLB, 
        //          f.IndicaMarco, 
        //          f.IndicaTarefaSumario, 
        //          f.Nivel, 
        //          f.IndicaCritica, 
        //          f.TerminoReal, 
        //          f.Duracao,
        //          f.InicioPrevisto, 
        //          f.TerminoPrevisto, 
        //          f.Predecessoras, 
        //          f.CodigoRealTarefa, 
        //          f.CodigoProjeto, 
        //          f.SequenciaTarefaCronograma,
        //          f.TarefaSuperior, 
        //          f.Desvio, 
        //          f.PercentualPrevisto, 
        //          f.CodigoCronogramaProjeto
        //          {9}
        //          {10}	
        //     FROM {0}.{1}.[f_GetCronogramaGanttProjeto] (@in_CodigoProjeto, @in_VersaoLinhaBase, @in_CodigoRecurso, @in_SoAtrasadas, @in_SoMarcos, @in_PercentualConcluido
        //       ,@in_DataFiltro) as f
        //   ORDER BY CodigoProjeto, CodigoCronogramaProjeto, [SequenciaTarefaCronograma]                                                                  
        //    END ", /*{0}*/bancodb,
        //    /*{1}*/Ownerdb,
        //    /*{2}*/codigoProjeto,
        //    /*{3}*/codigoRecurso,
        //    /*{4}*/-1,
        //    /*{5}*/(somenteAtrasadas == true) ? "S" : "N",
        //    /*{6}*/(somenteMarcos == true) ? "S" : "N",
        //    /*{7}*/(!percentualConcluido.HasValue) ? "NULL" : percentualConcluido.ToString(),
        //    /*{8}*/(!dataFiltro.HasValue) ? "NULL" : "CONVERT(DateTime,'" + dataFiltro.Value + "', 103)",
        //    /*{9}*/apareceColuna_ValorPesoTarefaLB,
        //    /*{10}*/apareceColuna_PercentualPesoTarefa);
        //    return getDataSet(comandoSQL);
        //}
        //else
        //{
            string innerJoinLB = "";

            if (versaoLB != -1)
            {
                innerJoinLB = string.Format(@" INNER JOIN
                               {0}.{1}.CronogramaProjeto AS cp ON (cp.CodigoProjeto = f.CodigoProjeto) LEFT JOIN
                               {0}.{1}.TarefaCronogramaProjetoLinhaBase AS lb ON (lb.CodigoCronogramaProjeto = cp.CodigoCronogramaProjeto
                                                                                AND lb.CodigoTarefa = f.CodigoNumeroTarefa
                                                                                AND lb.VersaoLinhaBase = {2})", bancodb, Ownerdb, versaoLB + 1);
            }

            string comandoSQL = string.Format(
                 @"BEGIN 
                DECLARE @in_CodigoProjeto as int
                DECLARE @in_VersaoLinhaBase as smallint
                DECLARE @in_CodigoRecurso as int
                DECLARE @in_SoAtrasadas as char(1)
                DECLARE @in_SoMarcos as  char(1)
                DECLARE @in_PercentualConcluido  int
                DECLARE @in_DataFiltro  datetime

                SET @in_CodigoProjeto = {2}
                SET @in_VersaoLinhaBase = {4}
                SET @in_CodigoRecurso =  {3}
                SET @in_SoAtrasadas = '{5}'
                SET @in_SoMarcos = '{6}'
                SET @in_PercentualConcluido = {7}
                SET @in_DataFiltro = {8} 
  
                SELECT 
	             f.EstruturaHierarquica
                           , f.CodigoTarefa
                           , f.NomeTarefa
                           , f.Concluido
                           , f.Trabalho
                           , f.Custo
                           , f.Inicio 
                           , f.Termino
                           , {11} AS InicioLB
                           , {12} AS TerminoLB
                           , f.IndicaMarco
                           , f.IndicaTarefaSumario
                           , f.Nivel
                           , f.IndicaCritica
                           , f.TerminoReal
                           , f.Duracao
                           , f.InicioPrevisto
                           , f.TerminoPrevisto
                           , f.Predecessoras
                           , f.CodigoRealTarefa
                           , f.CodigoProjeto
                           , f.SequenciaTarefaCronograma
                           , f.TarefaSuperior
                           , DATEDIFF(day, f.TerminoPrevisto, Termino) AS Desvio
                           , f.PercentualPrevisto
                           , f.CodigoCronogramaProjeto
                           , f.StringAlocacaoRecursoTarefa
                             {9}
                             {10}	
           FROM {0}.{1}.[f_GetCronogramaGanttProjeto] (@in_CodigoProjeto, @in_VersaoLinhaBase, @in_CodigoRecurso, @in_SoAtrasadas, @in_SoMarcos, @in_PercentualConcluido
               ,@in_DataFiltro) as f
           ORDER BY CodigoProjeto, SequenciaTarefaCronograma                                                                         
            END ", /*{0}*/bancodb,
            /*{1}*/Ownerdb,
            /*{2}*/codigoProjeto,
            /*{3}*/codigoRecurso,
            /*{4}*/versaoLB,
            /*{5}*/(somenteAtrasadas == true) ? "S" : "N",
            /*{6}*/(somenteMarcos == true) ? "S" : "N",
            /*{7}*/(!percentualConcluido.HasValue) ? "NULL" : percentualConcluido.ToString(),
            /*{8}*/(!dataFiltro.HasValue) ? "NULL" : "CONVERT(DateTime,'" + dataFiltro.Value + "', 103)",
            /*{9}*/apareceColuna_ValorPesoTarefaLB,
            /*{10}*/apareceColuna_PercentualPesoTarefa,
            /*{11}*/innerJoinLB != "" ? "f.InicioLB" : "f.InicioPrevisto",
            /*{12}*/ innerJoinLB != "" ? "f.TerminoLB" : "f.TerminoPrevisto");
            return getDataSet(comandoSQL);


        //}
    }

    public string getPredecessorasTasques(string codigoProjeto, string codigoTarefa)
    {
        string predecessoras = "";

        string comandoSQL = string.Format(
              @"SELECT CONVERT(VarChar, [indiceTarefaPredecessora]) + [tipoLatencia] + [latencia] + [formatoDuracaoLatencia] AS Predecessora
                  FROM {0}.{1}.[TarefaCronogramaProjetoPredecessoras] tcpp INNER JOIN
			           {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = tcpp.CodigoCronogramaProjeto 
													AND cp.CodigoProjeto = {2})
                 WHERE CodigoTarefa = {3}
               ", bancodb, Ownerdb, codigoProjeto, codigoTarefa);

        DataSet dsCrono = getDataSet(comandoSQL);

        if (DataSetOk(dsCrono) && DataTableOk(dsCrono.Tables[0]))
        {
            foreach (DataRow dr in dsCrono.Tables[0].Rows)
            {
                if (dr["Predecessora"].ToString() != "")
                    predecessoras += dr["Predecessora"].ToString() + ";";
            }
        }

        if (predecessoras != "")
            predecessoras = predecessoras.Substring(0, predecessoras.Length - 1);

        return predecessoras;
    }

    public bool indicaCronogramaVersaoTasques(int codigoProjeto)
    {
        bool retorno = false;

        string comandoSQL = string.Format(
             @"SELECT versaoDesktop FROM {0}.{1}.CronogramaProjeto WHERE CodigoProjeto = {2}
               ", bancodb, Ownerdb, codigoProjeto);

        DataSet dsCrono = getDataSet(comandoSQL);

        if (DataSetOk(dsCrono) && DataTableOk(dsCrono.Tables[0]))
            retorno = dsCrono.Tables[0].Rows[0]["versaoDesktop"].ToString().Contains("Portal") == false;

        return retorno;
    }

    public DataSet getDetalhesLinhaBase(int codigoProjeto, string versaoLinhaBase)
    {
        string replanejamentoTasques = "N", comandoSQL = "";

        DataSet dsParam = getParametrosSistema(int.Parse(getInfoSistema("CodigoEntidade").ToString()), "TASQUES_ReplanejarCronograma");

        if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
        {
            replanejamentoTasques = dsParam.Tables[0].Rows[0]["TASQUES_ReplanejarCronograma"].ToString();
        }

        if (replanejamentoTasques != "S")
        {
            comandoSQL = string.Format(
             @"BEGIN
	                  DECLARE @DataUltimaLB DateTime,
				              @CodigoProjeto int
				  
                      DECLARE @tbResumo Table(Versao int,
											DataAprovacao DateTime,
											Descricao VarChar(20),
											StatusLB VarChar(30),
											DataSolicitacao DateTime,
											CodigoSolicitante int,
											Anotacao VarChar(2000),
											CodigoAprovador int)
	
	                    SET @CodigoProjeto = {2}
	
	                    SELECT @DataUltimaLB = MAX(lbc.DataStatusAprovacao)
	                      FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
				               {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
													AND cp.CodigoProjeto = @CodigoProjeto)
	                     WHERE lbc.StatusAprovacao = 'AP'
	 
	                    INSERT INTO @tbResumo
	                     SELECT -1, DataStatusAprovacao, 'Versão Atual', 'Aprovada', DataSolicitacao, CodigoUsuarioSolicitante
				               , Anotacoes, CodigoUsuarioAprovacao
	                       FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
				                {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
	                     WHERE cp.CodigoProjeto = @CodigoProjeto	
		                     AND lbc.StatusAprovacao = 'AP'
		                     AND DataStatusAprovacao = @DataUltimaLB
	 
	                     INSERT INTO @tbResumo
		                    SELECT VersaoLinhaBase, DataStatusAprovacao, 'Versão ' + CONVERT(VarChar, VersaoLinhaBase)
				                  ,Case WHEN StatusAprovacao = 'AP' THEN 'Aprovada' WHEN StatusAprovacao = 'RP' then 'Reprovada' ELSE 'Pendente de Aprovação' END
				                  ,DataSolicitacao, CodigoUsuarioSolicitante, Anotacoes, CodigoUsuarioAprovacao 
			                 FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
					              {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
		                    WHERE cp.CodigoProjeto = @CodigoProjeto	
			                  AND DataStatusAprovacao <> @DataUltimaLB
			                  AND lbc.StatusAprovacao IN('AP', 'PA', 'RP')
                              AND EXISTS(SELECT 1 
                                          FROM {0}.{1}.TarefaCronogramaProjetoLinhaBase tcpl 
                                         WHERE tcpl.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto 
                                           AND tcpl.VersaoLinhaBase = (lbc.VersaoLinhaBase + 1))
		                    ORDER BY VersaoLinhaBase Desc
	
	 
	                     SELECT Versao, DataAprovacao, Descricao, StatusLB
				               ,DataSolicitacao, so.NomeUsuario AS Solicitante
				               ,ap.NomeUsuario AS Aprovador, Anotacao
                         FROM @tbResumo tb INNER JOIN
					          {0}.{1}.Usuario so ON so.CodigoUsuario = tb.CodigoSolicitante LEFT JOIN
					          {0}.{1}.Usuario ap ON ap.CodigoUsuario = tb.CodigoAprovador
                        WHERE Versao = {3}
	
                END
               ", bancodb, Ownerdb, codigoProjeto, versaoLinhaBase);
        }
        else
        {
            comandoSQL = string.Format(
                 @"BEGIN	               
                    DECLARE @CodigoProjeto INT

	                SET @CodigoProjeto = {2}
	                    
	                SELECT NumeroVersao AS Versao, DataStatusAprovacao AS DataAprovacao
					        ,CASE WHEN NumeroVersao = -1 THEN 'Versão Atual' ELSE 'Versão ' + CONVERT(VarChar, VersaoLinhaBase) END AS Descricao
					        ,Case WHEN StatusAprovacao = 'AP' THEN 'Aprovada' WHEN StatusAprovacao = 'RP' then 'Reprovada' ELSE 'Pendente de Aprovação' END as StatusLB
					        ,DataSolicitacao AS DataSolicitacao, vlb.NomeSolicitante AS Solicitante
					        ,NomeAprovador AS Aprovador, Anotacoes AS Anotacao 
		             FROM {0}.{1}.f_crono_GetVersoesLBProjeto(@CodigoProjeto) vlb
		            WHERE VersaoLinhaBase = {3}
                END
               ", bancodb, Ownerdb, codigoProjeto, versaoLinhaBase);
        }

        return getDataSet(comandoSQL);
    }

    public DataSet getVersoesLinhaBase(int codigoProjeto, string where)
    {
        string replanejamentoTasques = "N", comandoSQL = "";

        DataSet dsParam = getParametrosSistema(int.Parse(getInfoSistema("CodigoEntidade").ToString()), "TASQUES_ReplanejarCronograma");

        if (DataSetOk(dsParam) && DataTableOk(dsParam.Tables[0]))
            replanejamentoTasques = dsParam.Tables[0].Rows[0]["TASQUES_ReplanejarCronograma"].ToString();

        if (replanejamentoTasques != "S")
        {
            comandoSQL = string.Format(
             @"BEGIN
	                  DECLARE @DataUltimaLB DateTime,
				              @CodigoProjeto int
				  
                      DECLARE @tbResumo Table(Versao int,
											Descricao VarChar(20),
                                            StatusAprovacao Varchar(30))
	
	                    SET @CodigoProjeto = {2}
	
	                    SELECT @DataUltimaLB = MAX(lbc.DataStatusAprovacao)
	                      FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
				               {0}.{1}.CronogramaProjeto cp ON (cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
													AND cp.CodigoProjeto = @CodigoProjeto)
	                     WHERE lbc.StatusAprovacao = 'AP'
	 
	                    INSERT INTO @tbResumo
	                     SELECT -1, '" + Resources.traducao.vers_o_atual + @"', '" + Resources.traducao.aprovado + @"'
	                       FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
				                {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
	                     WHERE cp.CodigoProjeto = @CodigoProjeto	
		                     AND lbc.StatusAprovacao = 'AP'
		                     AND DataStatusAprovacao = @DataUltimaLB
	 
	                     INSERT INTO @tbResumo
		                    SELECT VersaoLinhaBase, '" + Resources.traducao.vers_o_lb_ + @"' + CONVERT(VarChar, VersaoLinhaBase)
                                  ,Case WHEN StatusAprovacao = 'AP' THEN '" + Resources.traducao.aprovado + @"'
                                        WHEN StatusAprovacao = 'RP' THEN '" + Resources.traducao.reprovado + @"'
                                        ELSE '" + Resources.traducao.pendente_de_aprova__o + @"' END
			                 FROM {0}.{1}.LinhaBaseCronograma lbc INNER JOIN
					              {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto
		                    WHERE cp.CodigoProjeto = @CodigoProjeto	
			                  AND DataStatusAprovacao <> @DataUltimaLB
			                  AND lbc.StatusAprovacao IN('AP', 'PA','RP')
                             AND EXISTS(SELECT 1 
                                          FROM {0}.{1}.TarefaCronogramaProjetoLinhaBase tcpl 
                                         WHERE tcpl.CodigoCronogramaProjeto = lbc.CodigoCronogramaProjeto 
                                           AND tcpl.VersaoLinhaBase = (lbc.VersaoLinhaBase + 1))
		                    ORDER BY VersaoLinhaBase Desc
	
	 
	                     SELECT Versao as NumeroVersao, Descricao as VersaoLinhaBase, StatusAprovacao
                         FROM @tbResumo tb
                        WHERE 1 = 1
                          {3}
	
                END
               ", bancodb, Ownerdb, codigoProjeto, where);
        }
        else
        {
            comandoSQL = string.Format(
                  @"BEGIN	               
                    DECLARE @CodigoProjeto INT
   	
	                SET @CodigoProjeto = {2}
	                    
	                SELECT VersaoLinhaBase AS NumeroVersao, CASE WHEN NumeroVersao = -1 THEN '" + Resources.traducao.vers_o_atual + @"' ELSE '" + Resources.traducao.vers_o + @" ' + CONVERT(VarChar, VersaoLinhaBase) END AS VersaoLinhaBase
                          ,Case WHEN StatusAprovacao = 'AP' THEN '" + Resources.traducao.aprovado + @"' WHEN StatusAprovacao = 'RP' THEN '" + Resources.traducao.reprovado + @"' ELSE '" + Resources.traducao.pendente_de_aprova__o + @"' END AS StatusAprovacao
                      FROM {0}.{1}.f_crono_GetVersoesLBProjeto(@CodigoProjeto) vlb
                     WHERE 1 = 1
                       {3}
	                ORDER BY CASE WHEN vlb.NumeroVersao = -1 THEN 9999 ELSE vlb.NumeroVersao END DESC
                END
               ", bancodb, Ownerdb, codigoProjeto, where);
        }

        return getDataSet(comandoSQL);
    }

    public DataSet getCronogramaGanttObjetivoEstrategico(int codigoObjetivo, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoObjetivo int       
        	
	                SET @CodigoObjetivo = {2}
                    
                    SELECT CodigoNumeroTarefa AS CodigoTarefa, NomeTarefa, PercentualReal AS Concluido, Trabalho, Custo, 
                           Inicio, InicioPrevisto,
                           Termino, TerminoPrevisto,
                           Marco AS IndicaMarco, TarefaResumo AS IndicaTarefaSumario, Nivel, IndicaCritica, TerminoReal, Duracao,
                           InicioPrevisto AS InicioLB, TerminoPrevisto AS TerminoLB, Predecessoras, CodigoTarefa AS CodigoRealTarefa, CodigoProjeto, 
                           SequenciaTarefaCronograma, f.TarefaSuperior, f.PercentualPrevisto
                      FROM {0}.{1}.f_GetCronogramaObjetivoEstrategico(@CodigoObjetivo) f
                      WHERE Inicio IS NOT NULL
					    AND Termino IS NOT NULL
                       {3}
                      ORDER BY EstruturaHierarquica
                    END
               ", bancodb, Ownerdb, codigoObjetivo, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCronogramaGanttPlanoAcao(int codigoObjetoAssociado, int codigoEntidade, string iniciaisTipoAssociacao, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT * FROM {0}.{1}.f_GetCronogramaPA('{2}', {3}, {4})
                WHERE 1 = 1 
                  {5}               
                ORDER BY EstruturaHierarquica, Inicio
               ", bancodb, Ownerdb, iniciaisTipoAssociacao, codigoObjetoAssociado, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGanttObras(int codigoUsuario, int codigoEntidade, int codigoCarteira, int anoContrato, string where)
    {
        string comandoSQL = string.Format(
               @"BEGIN
                      DECLARE @tblRetorno TABLE
                               (Codigo Int,
                                Descricao Varchar(255),
                                Inicio DateTime,
                                Termino DateTime,
                                Cor Varchar(20),
                                Concluido Decimal(5,2),
                                CodigoPai Int,
                                CodigoMunicipio Int,
                                Valor Decimal(18,4),
                                Saldo Decimal(18,4))
                        
                        /* Insere os projetos */
                        INSERT INTO @tblRetorno
                                     (Codigo,
                                      Descricao,
                                      Inicio,
                                      Termino,
                                      Cor,
                                      Concluido,
                                      CodigoPai,
                                      CodigoMunicipio,
                                      Valor,
                                      Saldo)
	                                SELECT p.CodigoProjeto AS Codigo, 
		                                   f.Projeto AS Descricao, 
		                                   p.InicioReprogramado AS Inicio, 
		                                   p.TerminoReprogramado AS Termino, 
		                                   p.CorGeral AS Cor, 
		                                   p.PercentualRealizacao * 100 AS Concluido,
		                                   o.CodigoMunicipioObra * -1 AS CodigoPai,
		                                   o.CodigoMunicipioObra,
		                                   f.ValorContrato,
		                                   f.Saldo
	                      FROM {0}.{1}.f_obr_GetDetalhesObras({3},{2}) F INNER JOIN
														{0}.{1}.ResumoProjeto p ON (p.CodigoProjeto = f.CodigoProjeto) INNER JOIN 
		                        {0}.{1}.f_getProjetosUsuario({3}, {2}, {4}) fu ON fu.CodigoProjeto = p.CodigoProjeto INNER JOIN
		                        {0}.{1}.Obra o ON o.CodigoProjeto = f.CodigoProjeto
	                     WHERE 1 = 1
			               {5}
                    	 
	                     /* Insere as municipios */
	                     INSERT INTO @tblRetorno
	                     (Codigo,
                          Descricao,
                          Inicio,
                          Termino,
                          Cor)
                          SELECT mo.CodigoMunicipio * -1,
                                 m.NomeMunicipio,
                                 MIN(t.Inicio),
                                 MAX(t.Termino),
                                 'Cinza'
                            FROM {0}.{1}.MunicipioObra AS mo INNER JOIN
																 {0}.{1}.Municipio m ON m.CodigoMunicipio = mo.CodigoMunicipio INNER JOIN
                                 @tblRetorno AS t ON (t.CodigoMunicipio = mo.CodigoMunicipio)                                                             
                          GROUP BY mo.CodigoMunicipio * -1,
                                   m.NomeMunicipio                        
                                                        
                        SELECT t.Codigo,
                               t.Descricao,
                               t.Inicio,
                               t.Termino,
                               t.Cor,
                               t.CodigoPai,
                               t.Concluido,
                               SUM(t.valor) AS Valor,
                               SUM(t.Saldo) AS Saldo,
                               CASE WHEN t.Inicio IS NULL OR t.Termino IS NULL THEN 'S' ELSE 'N' END AS SemCronograma
                          FROM @tblRetorno AS t 
                         GROUP BY t.Codigo,
                               t.Descricao,
                               t.Inicio,
                               t.Termino,
                               t.Cor,
                               t.CodigoPai,
                               t.Concluido
                         ORDER BY t.CodigoPai, t.Inicio, t.Termino        
                    END",
               bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    public string getGraficoGanttProjetos(DataTable dt, string tituloAgrupamento)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""340"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>" + /*tituloAgrupamento*/  @"/Projeto</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                        </columns>
                                      </datagrid>
                                     <settings>
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""2"">
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");

        xml.Append(string.Format(@"<styles>
				          <task_styles>
				            <task_style name=""VERDE"">
                               <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{0}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                        <task_style name=""AMARELO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{1}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                            <task_style name=""VERMELHO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{2}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                          <task_style name=""CINZA"">
    				            <tooltip enabled=""true"">
                                <text> {3}                                    
                                </text>
                              </tooltip> 
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text>{3}</text> 
                              </tooltip>
                              </row_datagrid>  
                                <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#BEB4EB"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4577FA"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                   <labels>
                                        <label anchor=""Right"" halign=""Far"" valign=""Center""> 
                                             <text></text>
                                        </label>
                                    </labels> 
                                </bar_style>
                             </actual>	                            			
				            </task_style>
                            <task_style name=""SC"">
				  				               <tooltip enabled=""true"">
				                                  <text><![CDATA[ {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                <row_datagrid>
				                                  <tooltip enabled=""true"">
				                                  <text><![CDATA[ {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                </row_datagrid>				  				               
				  						<actual>
		                                 <bar_style>
		                                  <middle shape=""Circle"" /> 
		                                 <start>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </start>
		                                 <end>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </end>
		                                  </bar_style>
                                  </actual>
				            </task_style>
				          </task_styles>
				        </styles>", corSatisfatorio, corAtencao, corCritico, "{%Name}", "{%ActualStart}", "{%ActualEnd}", "{%Complete}", tituloAgrupamento));

        xml.Append(@"<project_chart>
                      <tasks>
");

        foreach (DataRow dr in dt.Rows)
        {
            string inicio = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" || dr["Inicio"].ToString() == "" ? DateTime.Now : DateTime.Parse(dr["Inicio"].ToString()));

            string termino = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" || dr["Termino"].ToString() == "" ? DateTime.Now : DateTime.Parse(dr["Termino"].ToString()));

            xml.Append(string.Format(@"<task id=""{0}"" name=""{1}"" parent=""{2}"" actual_start=""{3}"" {7}=""{4}"" progress=""{5:n0}%"" style=""{6}""/>
"
                , dr["Codigo"].ToString()
                , dr["Descricao"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , dr["CodigoPai"].ToString()
                , inicio
                , termino
                , dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString())
                , dr["SemCronograma"] + "" == "S" || dr["Inicio"].ToString() == "" || dr["Termino"].ToString() == "" ? "SC" : dr["Cor"].ToString().Trim().ToUpper()
                , dr["SemCronograma"] + "" == "S" || dr["Inicio"].ToString() == "" || dr["Termino"].ToString() == "" ? "actual_end1" : "actual_end"));
        }

        xml.Append(string.Format(@"</tasks>
                                </project_chart>
                               </anygantt>"));

        return xml.ToString();
    }

    public string getGraficoGanttObras(DataTable dt)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""340"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Município/Obra</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                           <column width=""140"" cell_align=""Right"">
                                            <header>
                                              <text>Valor do Contrato(R$)</text>
                                            </header>
                                            <format>{%ValorContrato}</format>
                                          </column>
                                           <column width=""140"" cell_align=""Right"">
                                            <header>
                                              <text>Saldo Contratual(R$)</text>
                                            </header>
                                            <format>{%SaldoContratual}</format>
                                          </column>
                                        </columns>
                                      </datagrid>
                                     <settings>
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""true"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print print_all_columns=""True"" >
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");

        xml.Append(string.Format(@"<styles>
				          <task_styles>
				            <task_style name=""VERDE"">
                               <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{0}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                        <task_style name=""AMARELO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{1}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                            <task_style name=""VERMELHO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Obra: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{2}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                          <task_style name=""CINZA"">
    				            <tooltip enabled=""true"">
                                <text>Município: {3}                                    
                                </text>
                              </tooltip> 
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text>Município: {3}</text> 
                              </tooltip>
                              </row_datagrid>  
                                <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#BEB4EB"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4577FA"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>	                            			
				            </task_style>
                            <task_style name=""SC"">
				  				               <tooltip enabled=""true"">
				                                  <text><![CDATA[ Obra: {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                <row_datagrid>
				                                  <tooltip enabled=""true"">
				                                  <text><![CDATA[ Obra: {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                </row_datagrid>				  				               
				  						<actual>
		                                 <bar_style>
		                                  <middle shape=""Circle"" /> 
		                                 <start>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </start>
		                                 <end>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </end>
		                                  </bar_style>
                                  </actual>
				            </task_style>
				          </task_styles>
				        </styles>", corSatisfatorio, corAtencao, corCritico, "{%Name}", "{%ActualStart}", "{%ActualEnd}", "{%Complete}"));

        xml.Append(@"<project_chart>
                      <tasks>");

        foreach (DataRow dr in dt.Rows)
        {
            string inicio = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" ? DateTime.Now : DateTime.Parse(dr["Inicio"].ToString()));

            string termino = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" ? DateTime.Now : DateTime.Parse(dr["Termino"].ToString()));

            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" parent=""{2}"" actual_start=""{3}"" {7}=""{4}"" progress=""{5:n0}%"" style=""{6}"">
                                         <attributes>
                                          <attribute name=""ValorContrato"">{8} </attribute>
                                          <attribute name=""SaldoContratual"">{9} </attribute>    
                                        </attributes>
                                        </task>"
                , dr["Codigo"].ToString()
                , dr["Descricao"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , dr["CodigoPai"].ToString()
                , inicio
                , termino
                , dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString())
                , dr["SemCronograma"] + "" == "S" ? "SC" : dr["Cor"].ToString().Trim().ToUpper()
                , dr["SemCronograma"] + "" == "S" ? "actual_end1" : "actual_end"
                , dr["Valor"].ToString() == "" ? "-" : string.Format("{0:n2}", double.Parse(dr["Valor"].ToString()))
                , dr["Saldo"].ToString() == "" ? "-" : string.Format("{0:n2}", double.Parse(dr["Saldo"].ToString()))));
        }

        xml.Append(string.Format(@"</tasks>
                                </project_chart>
                               </anygantt>"));

        return xml.ToString();
    }

    public string getGraficoGanttSimulacao(DataTable dt)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""340"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Projeto</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                        </columns>
                                      </datagrid>
                                     <settings>
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""2"">
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");

        xml.Append(string.Format(@"<styles>
				          <task_styles>
				            <task_style name=""VERDE"">
                               <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{0}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                        <task_style name=""AMARELO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{1}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                            <task_style name=""VERMELHO"">
				               <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text><![CDATA[ Projeto: {3}
                                    ]]>
                                </text>
                              </tooltip>
                              </row_datagrid>
				               <progress>
					          <bar_style>
					            <middle>
					              <fill enabled=""true"" color=""#{2}"" />
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>
						<actual>
						  <bar_style>
						    <middle shape=""Full"">
						      <fill enabled=""true"" type=""Solid"" color=""#E0E0E0"" />
						    </middle>
						 </bar_style>
						</actual>
				            </task_style>
                          <task_style name=""CINZA"">
    				            <tooltip enabled=""true"">
                                <text>Projeto: {3}                                    
                                </text>
                              </tooltip> 
                              <row_datagrid>
                                <tooltip enabled=""true"">
                                <text>Projeto: {3}</text> 
                              </tooltip>
                              </row_datagrid>  
                                <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#BEB4EB"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4577FA"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4577FA""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>	                            			
				            </task_style>
                            <task_style name=""SC"">
				  				               <tooltip enabled=""true"">
				                                  <text><![CDATA[ Projeto: {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                <row_datagrid>
				                                  <tooltip enabled=""true"">
				                                  <text><![CDATA[ Projeto: {3}
				                                      ]]>
				                                  </text>
				                                </tooltip>
				                                </row_datagrid>				  				               
				  						<actual>
		                                 <bar_style>
		                                  <middle shape=""Circle"" /> 
		                                 <start>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </start>
		                                 <end>
		                                 <marker type=""Circle"">
		                                  <fill type=""Solid"" color=""#0016B5"" /> 
		                                  <border color=""#0016B5"" /> 
		                                  </marker>
		                                  </end>
		                                  </bar_style>
                                  </actual>
				            </task_style>
				          </task_styles>
				        </styles>", corSatisfatorio, corAtencao, corCritico, "{%Name}", "{%ActualStart}", "{%ActualEnd}", "{%Complete}"));

        xml.Append(@"<project_chart>
                      <tasks>");

        foreach (DataRow dr in dt.Rows)
        {
            string inicio = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" ? DateTime.Now : DateTime.Parse(dr["Inicio"].ToString()));

            string termino = string.Format(@"{0:dd/MM/yyyy HH:mm:ss}",
                dr["SemCronograma"] + "" == "S" ? DateTime.Now : DateTime.Parse(dr["Termino"].ToString()));

            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" parent="""" actual_start=""{2}"" {6}=""{3}"" style=""{4}"" progress=""{5:n0}%""/> "
                , dr["Codigo"].ToString()
                , dr["Descricao"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , inicio
                , termino
                , dr["SemCronograma"] + "" == "S" ? "SC" : dr["Cor"].ToString().Trim().ToUpper()
                , dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString())
                , dr["SemCronograma"] + "" == "S" ? "actual_end1" : "actual_end"));
        }

        xml.Append(string.Format(@"</tasks>
                                </project_chart>
                               </anygantt>"));

        return xml.ToString();
    }

    public string getGraficoGanttTarefasProjeto(DataTable dt, bool removerIdentacao, bool indicaTasques)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        #region Configurações do Gantt
        /*
        [ValorPesoTarefaLB], título "Peso LB" com 2 casas decimais;
    [PercentualPesoTarefa], título "% Peso" com 2 casas decimais.
    */
        bool permiteUsoPesoManual = false;
        DataSet ds = getParametrosSistema("TASQUES_PermiteUsoPesoManual");
        if (DataSetOk(ds))
        {
            permiteUsoPesoManual = ds.Tables[0].Rows[0]["TASQUES_PermiteUsoPesoManual"].ToString().Trim().ToUpper() == "S";
        }

        string novasColunasDePesoTarefa = permiteUsoPesoManual == true ? @"<column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>Peso LB</text>
                                            </header>
                                            <format>{%ValorPesoTarefaLB}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>% Peso</text>
                                            </header>
                                            <format>{%PercentualPesoTarefa}</format>
                                          </column>" : "";


        xml.Append(@"<anygantt>
                                     <datagrid width=""570"">
                                        <columns>  
                                          <column width=""50"" cell_align=""Right"">
                                            <header>
                                              <text>#</text>
                                            </header>
                                            <format>{%Sequencia}</format>
                                          </column>     
                                          <column width=""320"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Tarefa</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>                                            
                                          <column width=""120"" cell_align=""Center"">
                                            <header>
                                              <text>Início LB</text>
                                            </header>
                                            <format>{%InicioLB}</format>
                                          </column>
                                          <column width=""120"" cell_align=""Center"">
                                            <header>
                                              <text>Término LB</text>
                                            </header>
                                            <format>{%TerminoLB}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>Previsto</text>
                                            </header>
                                            <format>{%Previsto}</format>
                                          </column>
                                          <column width=""95"" cell_align=""Right"">
                                            <header>
                                              <text>Realizado</text>
                                            </header>
                                            <format>{%Complete}%</format>
                                          </column>"
                                           + novasColunasDePesoTarefa +
                                          @"<column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>Duração(d)</text>
                                            </header>
                                            <format>{%Duracao}</format>
                                          </column>
                                          <column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>Trabalho(h)</text>
                                            </header>
                                            <format>{%Trabalho}</format>
                                          </column>                                         
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Término Real</text>
                                            </header>
                                            <format>{%TerminoReal}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                        </columns>
                                        <style interlaced=""true""> 
                                            <cell> 
                                            <even>
                                            <fill enabled=""true"" type=""Solid"" color=""#3D34B1"" >                                              
                                            </fill>                                            
                                          </even>
                                          <odd>
                                            <fill enabled=""true"" type=""Solid"" color=""White"">                                             
                                            </fill>
                                          </odd>
                                          </cell>
                                        </style>
                                      </datagrid>
                                     <settings>                                     
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" mode=""FitContent"" height=""100%""/>
                                     <context_menu save_as_image=""false"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""1"" mode=""SinglePage"">
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <plot line_height=""25"" item_height=""20"" >
                                         <grid>
                                         <horizontal>
                                          <line enabled=""true"" color=""DarkSeaGreen"" thickness=""1"" /> 
                                         <even>
                                          <fill enabled=""true"" color=""DarkSeaGreen"" opacity=""0.2"" /> 
                                          </even>
                                         <odd>
                                          <fill enabled=""true"" color=""White"" opacity=""1"" /> 
                                          </odd>
                                          </horizontal>
                                          </grid>
                                          </plot>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");
        #endregion

        xml.Append(getEstilos());

        xml.Append(@"<project_chart>
                      <tasks>");

        /// determina o modo de cálculo de atraso

        bool bModoCalculoAtrasoTotal = false;
        DataSet dsTemp = getParametrosSistema("calculoDesempenhoFisicoTarefa");
        if (DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0]))
        {
            string modoCalculoAtraso = dsTemp.Tables[0].Rows[0]["calculoDesempenhoFisicoTarefa"].ToString();
            bModoCalculoAtrasoTotal = modoCalculoAtraso.ToUpper().Equals("TOTAL");
        }

        foreach (DataRow dr in dt.Rows)
        { //

            string datasLinhaBase = "";

            try
            {
                datasLinhaBase = dr["InicioLB"].ToString() != "" && dr["TerminoLB"].ToString() != "" ? string.Format(@"baseline_start=""{0}"" baseline_end=""{1}""",
                     dr["InicioLB"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["InicioLB"].ToString()))
                    , dr["TerminoLB"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["TerminoLB"].ToString()))) : "";
            }
            catch { }

            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" {2} actual_start=""{3}"" {6}=""{4}"" {14} progress=""{5:n0}%"" {7}>
                                        <attributes>
                                          <attribute name=""Duracao"">{8:n2}</attribute>
                                          <attribute name=""Trabalho"">{9:n1}</attribute>                                          
                                          <attribute name=""Custo"">{10:c2}</attribute>
                                          <attribute name=""InicioPrevisto"">{11}</attribute>                                          
                                          <attribute name=""TerminoPrevisto"">{12}</attribute>
                                          <attribute name=""Previsto"">{13:n0}</attribute>
                                          <attribute name=""Sequencia"">{15:n0}</attribute>                                        
                                          <attribute name=""TerminoReal"">{16}</attribute>
                                          <attribute name=""InicioLB"">{17}</attribute>                                          
                                          <attribute name=""TerminoLB"">{18}</attribute>
                                          
                                          <attribute name=""ValorPesoTarefaLB"">{19}</attribute>
                                          <attribute name=""PercentualPesoTarefa"">{20}</attribute>
                                          
                                        </attributes></task>
 
 "
                , dr["CodigoRealTarefa"].ToString()
                , dr["NomeTarefa"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , "parent=\"" + ((dr["TarefaSuperior"].ToString() == dr["CodigoRealTarefa"].ToString()) ? "" : dr["TarefaSuperior"].ToString()) + "\""
                , dr["Inicio"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Inicio"].ToString()))
                , dr["Termino"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Termino"].ToString()))
                , dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString())
                , dr["IndicaMarco"] + "" == "True" || dr["IndicaMarco"] + "" == "1" ? "actual_end1" : "actual_end"
                , getStiloTarefa(dr, bModoCalculoAtrasoTotal)
                , dr["Duracao"] + "" == "" ? 0 : float.Parse(dr["Duracao"].ToString())
                , dr["Trabalho"] + "" == "" ? 0 : float.Parse(dr["Trabalho"].ToString())
                , dr["Custo"] + "" == "" ? 0 : float.Parse(dr["Custo"].ToString())
                , dr["InicioPrevisto"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["InicioPrevisto"].ToString()))
                , dr["TerminoPrevisto"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["TerminoPrevisto"].ToString()))
                , dr["PercentualPrevisto"] + "" == "" ? "-" : string.Format("{0:n0}%", dr["PercentualPrevisto"])
                , datasLinhaBase
                , float.Parse(dr["SequenciaTarefaCronograma"].ToString())
                , dr["TerminoReal"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["TerminoReal"].ToString()))
                , dr["InicioLB"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["InicioLB"].ToString()))
                , dr["TerminoLB"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["TerminoLB"].ToString()))
                , dr["ValorPesoTarefaLB"] + "" == "" ? "-" : string.Format("{0:n2}", decimal.Parse(dr["ValorPesoTarefaLB"].ToString()))
                , dr["PercentualPesoTarefa"] + "" == "" ? "-" : string.Format("{0:n2}%", decimal.Parse(dr["PercentualPesoTarefa"].ToString()))));
        }

        xml.Append(@"</tasks>");
        xml.Append(@"<connectors>");

        if (!removerIdentacao)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string codigoTarefa = dr["CodigoTarefa"].ToString();
                string predecessorasDb = "";

                if (indicaTasques)
                {
                    predecessorasDb = getPredecessorasTasques(dr["CodigoProjeto"].ToString(), dr["CodigoTarefa"].ToString());
                }
                else
                {
                    predecessorasDb = dr["Predecessoras"].ToString();
                }

                if (codigoTarefa != "" && predecessorasDb != "")
                {
                    string tarefaDestino = dr["CodigoRealTarefa"].ToString();

                    string[] aPredecessora = predecessorasDb.Split(';');

                    string tarefaOrigem = "";

                    foreach (string predecessora in aPredecessora)
                    {
                        if (predecessora != "")
                        {
                            int indexPredecessora;
                            string tipoConector = "";
                            if (predecessora.IndexOfAny(new char[] { 'T', 'I' }) > 0)
                            {
                                indexPredecessora = int.Parse(predecessora.Substring(0, predecessora.IndexOfAny(new char[] { 'T', 'I' })));
                                tipoConector = predecessora.Substring(predecessora.IndexOfAny(new char[] { 'T', 'I' }), 2);
                            }
                            else
                                indexPredecessora = int.Parse(predecessora);

                            string where = "SequenciaTarefaCronograma = " + indexPredecessora + " AND CodigoProjeto = " + dr["CodigoProjeto"];

                            DataRow[] drs = dt.Select(where);

                            if (drs.Length > 0)
                            {

                                tarefaOrigem = drs[0]["CodigoRealTarefa"].ToString();

                                if (tipoConector == "" || tipoConector == "TI")
                                    tipoConector = "FinishStart";
                                else if (tipoConector == "II")
                                    tipoConector = "StartStart";
                                else if (tipoConector == "TT")
                                    tipoConector = "FinishFinish";
                                else if (tipoConector == "IT")
                                    tipoConector = "StartFinish";

                                xml.Append(string.Format(@"<connector type=""{0}"" from=""{1}"" to=""{2}"" >", tipoConector, tarefaOrigem, tarefaDestino));

                                if (dr["IndicaCritica"].ToString() == "True" || dr["IndicaCritica"].ToString() == "1")
                                {
                                    xml.Append(@"<connector_style><line thickness=""1"" color=""#7342D7"" /></connector_style>");
                                }
                                else
                                {
                                    xml.Append(@"<connector_style><line thickness=""1"" color=""#000000"" /></connector_style>");
                                }
                                xml.Append("</connector>");
                            }
                        }
                    }
                }
            }
        }
        xml.Append(@"</connectors>
                   </project_chart>
                      </anygantt>");

        return xml.ToString();
    }

    //TODO -- COMENTAR
    public bool getModoCalculoAtrasoTotal()
    {
        bool bModoCalculoAtrasoTotal = false;
        DataSet dsTemp = getParametrosSistema("calculoDesempenhoFisicoTarefa");
        if (DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0]))
        {
            string modoCalculoAtraso = dsTemp.Tables[0].Rows[0]["calculoDesempenhoFisicoTarefa"].ToString();
            bModoCalculoAtrasoTotal = modoCalculoAtraso.ToUpper().Equals("TOTAL");
        }
        return bModoCalculoAtrasoTotal;
    }


    public string getClaseTarefa(DataRow dr, bool calculoDeAtrasoNoModoTotal)
    {
        string estilo = "";

        bool tarefaAtrasada = false;
        bool tarefaSumario = (dr["IndicaTarefaSumario"].ToString() == "True" || dr["IndicaTarefaSumario"].ToString() == "1");
        bool tarefaMarco = (dr["IndicaMarco"].ToString() == "True" || dr["IndicaMarco"].ToString() == "1");
        bool tarefaCritica = (dr["IndicaCritica"].ToString() == "True" || dr["IndicaCritica"].ToString() == "1");
        string terminoPrevisto = dr["TerminoPrevisto"].ToString();
        string terminoReal = dr["TerminoReal"].ToString();

        float concluido = 0;
        if (!string.IsNullOrWhiteSpace(dr["Concluido"].ToString()))
        {
            concluido = float.Parse(dr["Concluido"].ToString());
        }


        float previsto = dr["PercentualPrevisto"].ToString() != "" ? float.Parse(dr["PercentualPrevisto"].ToString()) : 0;

        if (terminoPrevisto == "")
        {
            terminoPrevisto = dr["Termino"].ToString();
        }

        if (calculoDeAtrasoNoModoTotal)
            tarefaAtrasada = (terminoPrevisto != "" && (DateTime.Parse(terminoPrevisto) < DateTime.Now) && terminoReal == "" && concluido < 100);
        else
            tarefaAtrasada = concluido < previsto;

        if (tarefaAtrasada)
        {
            if (tarefaMarco)
            {
                estilo = "atrasada marco";
            }
            else
            {
                if (tarefaCritica)
                {
                    estilo = (tarefaSumario) ? "atrasada critica sumario" : "atrasada critica";
                }
                else
                {
                    estilo = (tarefaSumario) ? "atrasada sumario" : "atrasada";
                }
            }
        }
        else if (concluido > previsto)
        {
            if (tarefaMarco)
            {
                estilo = "adiantada marco";
            }
            else
            {
                if (tarefaCritica)
                {
                    estilo = (tarefaSumario) ? "adiantada critica sumario" : "adiantada critica";
                }
                else
                {
                    estilo = (tarefaSumario) ? "adiantada sumario" : "adiantada";
                }
            }
        }
        else if (concluido == 100)
        {
            if (tarefaMarco)
                estilo = "marco concluida";
            else
                estilo = (tarefaSumario) ? "concluida sumario" : "concluida";
        }
        else
        {
           
            if (tarefaCritica)
            {
                if (tarefaMarco)
                    estilo = "marco critica";
                else
                    estilo = (tarefaSumario) ? "critica sumario" : "critica";
            }
            else
            {
                if (tarefaMarco)
                {
                    estilo = "marco";
                }
            }
        }
        return estilo;
    }


    public string getGraficoZoomGanttTarefasProjeto(DataTable dt, bool removerIdentacao, bool indicaTasques)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        #region Configurações do Gantt

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""520"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Tarefa</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>
                                        </columns>
                                        <style interlaced=""true""> 
                                            <cell> 
                                            <even>
                                            <fill enabled=""true"" type=""Solid"" color=""#3D34B1"" >                                              
                                            </fill>                                            
                                          </even>
                                          <odd>
                                            <fill enabled=""true"" type=""Solid"" color=""White"">                                             
                                            </fill>
                                          </odd>
                                          </cell>
                                        </style>
                                      </datagrid>
                                     <settings>                                     
                                     <image_export url=""./AnyChartPNGSaver.aspx"" mode=""FitContent"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""false"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""1"" mode=""SinglePage"">
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <plot line_height=""25"" item_height=""20"" >
                                         <grid>
                                         <horizontal>
                                          <line enabled=""true"" color=""DarkSeaGreen"" thickness=""1"" /> 
                                         <even>
                                          <fill enabled=""true"" color=""DarkSeaGreen"" opacity=""0.2"" /> 
                                          </even>
                                         <odd>
                                          <fill enabled=""true"" color=""White"" opacity=""1"" /> 
                                          </odd>
                                          </horizontal>
                                          </grid>
                                          </plot>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");
        #endregion

        xml.Append(getEstilos());

        xml.Append(@"<project_chart>
                      <tasks>");

        /// determina o modo de cálculo de atraso

        bool bModoCalculoAtrasoTotal = false;
        DataSet dsTemp = getParametrosSistema("calculoDesempenhoFisicoTarefa");
        if (DataSetOk(dsTemp) && DataTableOk(dsTemp.Tables[0]))
        {
            string modoCalculoAtraso = dsTemp.Tables[0].Rows[0]["calculoDesempenhoFisicoTarefa"].ToString();
            bModoCalculoAtrasoTotal = modoCalculoAtraso.ToUpper().Equals("TOTAL");
        }

        foreach (DataRow dr in dt.Rows)
        { //

            string datasLinhaBase = "";

            try
            {
                datasLinhaBase = dr["InicioLB"].ToString() != "" && dr["TerminoLB"].ToString() != "" ? string.Format(@"baseline_start=""{0}"" baseline_end=""{1}""",
                     dr["InicioLB"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["InicioLB"].ToString()))
                    , dr["TerminoLB"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["TerminoLB"].ToString()))) : "";
            }
            catch { }

            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" {2} actual_start=""{3}"" {6}=""{4}"" {14} progress=""{5:n0}%"" {7}>
                                        </task>
 
 "
                , dr["CodigoRealTarefa"].ToString()
                , dr["NomeTarefa"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , "parent=\"" + ((dr["TarefaSuperior"].ToString() == dr["CodigoRealTarefa"].ToString()) ? "" : dr["TarefaSuperior"].ToString()) + "\""
                , dr["Inicio"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Inicio"].ToString()))
                , dr["Termino"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Termino"].ToString()))
                , dr["Concluido"] + "" == "" ? 0 : float.Parse(dr["Concluido"].ToString())
                , dr["IndicaMarco"] + "" == "True" || dr["IndicaMarco"] + "" == "1" ? "actual_end1" : "actual_end"
                , getStiloTarefa(dr, bModoCalculoAtrasoTotal)
                , dr["Duracao"] + "" == "" ? 0 : float.Parse(dr["Duracao"].ToString())
                , dr["Trabalho"] + "" == "" ? 0 : float.Parse(dr["Trabalho"].ToString())
                , dr["Custo"] + "" == "" ? 0 : float.Parse(dr["Custo"].ToString())
                , dr["InicioPrevisto"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["InicioPrevisto"].ToString()))
                , dr["TerminoPrevisto"] + "" == "" ? "-" : string.Format("{0:dd/MM/yyyy}", DateTime.Parse(dr["TerminoPrevisto"].ToString()))
                , dr["PercentualPrevisto"] + "" == "" ? 0 : float.Parse(dr["PercentualPrevisto"].ToString())
                , datasLinhaBase));
        }

        xml.Append(@"</tasks>");
        xml.Append(@"<connectors>");

        if (!removerIdentacao)
        {
            foreach (DataRow dr in dt.Rows)
            {
                string codigoTarefa = dr["CodigoTarefa"].ToString();
                string predecessorasDb = "";

                if (indicaTasques)
                {
                    predecessorasDb = getPredecessorasTasques(dr["CodigoProjeto"].ToString(), dr["CodigoTarefa"].ToString());
                }
                else
                {
                    predecessorasDb = dr["Predecessoras"].ToString();
                }

                if (codigoTarefa != "" && predecessorasDb != "")
                {
                    string tarefaDestino = dr["CodigoRealTarefa"].ToString();

                    string[] aPredecessora = predecessorasDb.Split(';');

                    string tarefaOrigem = "";

                    foreach (string predecessora in aPredecessora)
                    {
                        if (predecessora != "")
                        {
                            int indexPredecessora;
                            string tipoConector = "";
                            if (predecessora.IndexOfAny(new char[] { 'T', 'I' }) > 0)
                            {
                                indexPredecessora = int.Parse(predecessora.Substring(0, predecessora.IndexOfAny(new char[] { 'T', 'I' })));
                                tipoConector = predecessora.Substring(predecessora.IndexOfAny(new char[] { 'T', 'I' }), 2);
                            }
                            else
                                indexPredecessora = int.Parse(predecessora);

                            string where = "SequenciaTarefaCronograma = " + indexPredecessora + " AND CodigoProjeto = " + dr["CodigoProjeto"];

                            DataRow[] drs = dt.Select(where);

                            if (drs.Length > 0)
                            {

                                tarefaOrigem = drs[0]["CodigoRealTarefa"].ToString();

                                if (tipoConector == "" || tipoConector == "TI")
                                    tipoConector = "FinishStart";
                                else if (tipoConector == "II")
                                    tipoConector = "StartStart";
                                else if (tipoConector == "TT")
                                    tipoConector = "FinishFinish";
                                else if (tipoConector == "IT")
                                    tipoConector = "StartFinish";

                                xml.Append(string.Format(@"<connector type=""{0}"" from=""{1}"" to=""{2}"" >", tipoConector, tarefaOrigem, tarefaDestino));

                                if (dr["IndicaCritica"].ToString() == "True" || dr["IndicaCritica"].ToString() == "1")
                                {
                                    xml.Append(@"<connector_style><line thickness=""1"" color=""#7342D7"" /></connector_style>");
                                }
                                else
                                {
                                    xml.Append(@"<connector_style><line thickness=""1"" color=""#000000"" /></connector_style>");
                                }
                                xml.Append("</connector>");
                            }
                        }
                    }
                }
            }
        }
        xml.Append(@"</connectors>
                   </project_chart>
                      </anygantt>");

        return xml.ToString();
    }

    public string getGraficoGanttPlanosAcao(DataTable dt, bool removerIdentacao)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        #region Configurações do Gantt

        xml.Append(@"<anygantt>
                                     <datagrid width=""520"">
                                        <columns>       
                                          <column width=""320"" cell_align=""LeftLevelPadding"">
                                            <header>
                                              <text>Tarefa</text>
                                            </header>
                                            <format>{%Name}</format>
                                          </column>                                            
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Início</text>
                                            </header>
                                            <format>{%PeriodStart}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>
                                          <column width=""100"" cell_align=""Center"">
                                            <header>
                                              <text>Término</text>
                                            </header>
                                            <format>{%PeriodEnd}{dateTimeFormat:%dd/%MM/%yyyy}</format>
                                          </column>                                          
                                          <column width=""90"" cell_align=""Right"">
                                            <header>
                                              <text>Trabalho(h)</text>
                                            </header>
                                            <format>{%Trabalho}</format>
                                          </column>
                                          <column width=""130"" cell_align=""Right"">
                                            <header>
                                              <text>Despesa</text>
                                            </header>
                                            <format>{%Custo} </format>
                                          </column>                                          
                                        </columns>
                                        <style interlaced=""true""> 
                                            <cell> 
                                            <even>
                                            <fill enabled=""true"" type=""Solid"" color=""#3D34B1"" >                                              
                                            </fill>                                            
                                          </even>
                                          <odd>
                                            <fill enabled=""true"" type=""Solid"" color=""White"">                                             
                                            </fill>
                                          </odd>
                                          </cell>
                                        </style>
                                      </datagrid>
                                     <settings>                                     
                                     <image_export url=""../../AnyChartPNGSaver.aspx"" />
                                     <context_menu save_as_image=""true"" version_info=""false"" print_chart=""true"" about_anygantt=""false"" >
                                        <save_as_image_item_text>Salvar Gráfico como Imagem</save_as_image_item_text>
                                        <print_chart_item_text>Imprimir Gráfico</print_chart_item_text>
                                    </context_menu>
                                         <print columns_on_every_page=""2"">
                                         </print>                                        
                                     <locale>
                                        <date_time_format week_starts_from_monday=""True"">
                                          <months>
                                            <names>Janeiro,Fevereiro,Março,Abril,Maio,Junho,Jullho,Agosto,Setembro,Outubro,Novembro,Dezembro</names>
                                            <short_names>Jan,Fev,Mar,Abr,Mai,Jun,Jul,Ago,Set,Out,Nov,Dez</short_names>
                                          </months>
                                          <week_days>
                                            <names>Domingo,Segunda,Terça,Quarta,Quinta,Sexta,Sábado</names>
                                            <short_names>Dom,Seg,Ter,Qua,Qui,Sex,Sab</short_names>
                                          </week_days>
                                          <format>
                                              <full>%dd/%MM/%yyyy %HH:%mm:%ss</full>
                                              <date>%dd/%MM/%yyyy</date>
                                              <time>%HH:%mm:%ss</time>
                                          </format>
                                        </date_time_format>
                                      </locale>
                                     <navigation enabled=""True"" position=""Top"" size=""30"">
                                      <buttons collapse_expand_button=""true"" align=""Far"" /> 
                                      <text>Gráfico de Gantt</text> 
                                      <font face=""Verdana"" size=""10"" bold=""true"" color=""White"" /> 
                                     <background>
                                     <fill type=""Gradient"">
                                     <gradient>
                                      <key color=""#B0B0B0"" position=""0"" /> 
                                      <key color=""#A0A0A0"" position=""0.3"" /> 
                                      <key color=""#999999"" position=""0.5"" /> 
                                      <key color=""#A0A0A0"" position=""0.7"" /> 
                                      <key color=""#B0B0B0"" position=""1"" /> 
                                      </gradient>
                                      </fill>
                                      <border type=""Solid"" color=""#494949"" /> 
                                      </background>
                                      </navigation>
                                      <background enabled=""false"" /> 
                                      </settings>
                                      <timeline>
                                      <plot>
                                         <grid>
                                         <horizontal>
                                          <line enabled=""true"" color=""DarkSeaGreen"" thickness=""1"" /> 
                                         <even>
                                          <fill enabled=""true"" color=""DarkSeaGreen"" opacity=""0.2"" /> 
                                          </even>
                                         <odd>
                                          <fill enabled=""true"" color=""White"" opacity=""1"" /> 
                                          </odd>
                                          </horizontal>
                                          </grid>
                                          </plot>
                                      <scale>
                                      <patterns>
                                        <weeks>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                        </weeks>
                                        <days>
                                          <pattern>%dd/%MM/%yy</pattern>
                                          <pattern>%dd/%MMM/%yy</pattern>
                                          <pattern>%dd/%MMMM/%yyyy</pattern>
                                          <pattern is_lower=""true"">%dd</pattern>
                                        </days>
                                        <months>
                                          <pattern is_lower=""true"">%MM</pattern>
                                          <pattern is_lower=""true"">%MMM</pattern>
                                          <pattern is_lower=""true"">%MMMM</pattern>
                                          <pattern>%MMM/%yy</pattern>
                                          <pattern>%MMMM/%yyyy</pattern>
                                        </months>
                                        <quarters>
                                          <pattern>T %q/%yyyy</pattern>
                                          <pattern>Trim %q/%yyyy</pattern>
                                          <pattern>Trimestre %q/%yyyy</pattern>
                                        </quarters>
                                        <semesters>
                                            <pattern is_lower=""true"">S %r</pattern>
                                        </semesters>
                                      </patterns>
                                    </scale>
                                    </timeline>
                                      ");
        #endregion

        xml.Append(getEstilosPA());

        xml.Append(@"<project_chart>
                      <tasks>");

        foreach (DataRow dr in dt.Rows)
        {
            xml.Append(string.Format(@" <task id=""{0}"" name=""{1}"" {2} actual_start=""{3}"" actual_end=""{4}"" progress=""{5:n0}%"" {6}>
                                        <attributes>
                                          <attribute name=""Trabalho"">{7:n1}</attribute>                                          
                                          <attribute name=""Custo"">{8:c2}</attribute>
                                        </attributes></task>
 
 "
                , dr["EstruturaHierarquica"].ToString()
                , dr["Descricao"].ToString().Replace("\"", "'").Replace("<", "&lt;").Replace(">", "&gt;").Replace("–", "-")
                , (removerIdentacao) ? "" : "level=\"" + (int.Parse(dr["Nivel"].ToString()) - 1) + "\""
                , dr["Inicio"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Inicio"].ToString()))
                , dr["Termino"] + "" == "" ? "" : string.Format("{0:dd/MM/yyyy HH:mm:ss}", DateTime.Parse(dr["Termino"].ToString()))
                , dr["PercentualConcluido"] + "" == "" ? 0 : float.Parse(dr["PercentualConcluido"].ToString())
                , getStiloPlanoAcao(dr)
                , dr["Trabalho"] + "" == "" ? 0 : float.Parse(dr["Trabalho"].ToString())
                , dr["Custo"] + "" == "" ? 0 : float.Parse(dr["Custo"].ToString())
                ));
        }

        xml.Append(@"</tasks>
                   </project_chart>
                      </anygantt>");

        return xml.ToString();
    }

    public string getGanttVazio(string altura)
    {
        return string.Format(@"<table cellpadding=""0"" cellspacing=""0"" style=""height:{0}px; width:100%; font-family:Verdana; font-size:8pt"">
                                                <tr>
                                                    <td align=""center"">" + Resources.traducao.dados_ainda_n_o_h__cronograma_planejado_para_ser_exibido + @"</td>
                                                </tr>
                                            </table>", altura);

    }

    private string getEstilos()
    {
        return string.Format(@"<styles>
                           <defaults>
                              <task>
                                <task_style>
                                    <row_datagrid>
                                        <cell>
                                          <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                                        </cell>
                                        <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                      </row_datagrid>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                             <progress>
					          <bar_style>
					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>		
                                <baseline>
<bar_style>
	<middle shape=""HalfCenter""> 
	<fill enabled=""true"" type=""Gradient"">
	<gradient>
	<key color=""#E1E1E1"" />
	<key color=""#A1A1A1"" />
	</gradient>
	</fill>
	</middle>
	<labels />
	</bar_style>
	</baseline>
</task_style>
                              </task>

                              <milestone>                                
                                <task_style>
                                <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                       
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                                <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                    </task_style>
                              </milestone>
                              <summary>
                                <task_style>
                                  <row_datagrid>
                                    <cell>
                                      <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                        
                                    </cell>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  </row_datagrid>
                                  <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  <progress>
                                    <bar_style>
                                      <middle shape=""HalfTop"">
                                        <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" opacity=""1"" />
                                      </middle>
                                    </bar_style>
                                  </progress>
                                </task_style>
                              </summary>
                            </defaults>
				          <task_styles>				          
                          <task_style name=""marco"">
                         <actual>
                                 <bar_style>
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual>
                          <row_datagrid>
                            <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            

<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""marcoCritico"">
                         <actual>
                                 <bar_style>
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual>
                          <row_datagrid>
                            <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            

<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""criticaSumario"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""critica"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfCenter"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""Full"">
						              <fill enabled=""true"" type=""Solid"" color=""#7342D7"" />
						            </middle>                                  
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""atrasadaCritica"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""red"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfCenter"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""Full"">
						              <fill enabled=""true"" type=""Solid"" color=""#7342D7"" />
						            </middle>                                  
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""atrasada"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""red"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""atrasadaSumario"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""red"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""atrasadaMarco"">
                          <actual>
                                 <bar_style>
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual> 
                          <row_datagrid>                           
                           <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" color=""red"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""atrasadaCriticaSumario"">
                          <row_datagrid>
                            <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" color=""red"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>

<task_style name=""adiantadaCritica"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""blue"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfCenter"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""Full"">
						              <fill enabled=""true"" type=""Solid"" color=""#7342D7"" />
						            </middle>                                  
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""adiantada"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""blue"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""adiantadaSumario"">
                          <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" color=""blue"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
                        <task_style name=""adiantadaMarco"">
                          <actual>
                                 <bar_style>
                                  <middle shape=""Rhomb"" /> 
                                 <start>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </start>
                                 <end>
                                 <marker type=""Rhomb"">
                                  <fill type=""Solid"" color=""#B9992B"" /> 
                                  <border color=""#454545"" /> 
                                  </marker>
                                  </end>
                                  </bar_style>
                                  </actual> 
                          <row_datagrid>                           
                           <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" color=""blue"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                        </task_style>
                        <task_style name=""adiantadaCriticaSumario"">
                          <row_datagrid>
                            <cell>
                              <font italic=""true"" face=""Verdana"" size=""11"" color=""blue"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                            </cell>
<tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
Início LB: {4}
Término LB: {5}
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                          <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#4A4A4A"" />
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>
                        </task_style>
                        </task_styles>
				        </styles>", "{%Name}", "{%ActualStart}{dateTimeFormat:%dd/%MM/%yyyy}", "{%ActualEnd}{dateTimeFormat:%dd/%MM/%yyyy}", "{%Complete}"
                                  , "{%BaselineStart}{dateTimeFormat:%dd/%MM/%yyyy}", "{%BaselineEnd}{dateTimeFormat:%dd/%MM/%yyyy}");
    }

    private string getStiloTarefa(DataRow dr, bool calculoDeAtrasoNoModoTotal)
    {
        string estilo = "";

        bool tarefaAtrasada = false;
        bool tarefaSumario = (dr["IndicaTarefaSumario"].ToString() == "True" || dr["IndicaTarefaSumario"].ToString() == "1");
        bool tarefaMarco = (dr["IndicaMarco"].ToString() == "True" || dr["IndicaMarco"].ToString() == "1");
        bool tarefaCritica = (dr["IndicaCritica"].ToString() == "True" || dr["IndicaCritica"].ToString() == "1");
        string terminoPrevisto = dr["TerminoPrevisto"].ToString();
        string terminoReal = dr["TerminoReal"].ToString();
        float concluido = float.Parse(dr["Concluido"].ToString());
        float previsto = dr["PercentualPrevisto"].ToString() != "" ? float.Parse(dr["PercentualPrevisto"].ToString()) : 0;

        if (terminoPrevisto == "")
        {
            terminoPrevisto = dr["Termino"].ToString();
        }

        if (calculoDeAtrasoNoModoTotal == true)
            tarefaAtrasada = (terminoPrevisto != "" && (DateTime.Parse(terminoPrevisto) < DateTime.Now) && terminoReal == "" && concluido < 100);
        else
            tarefaAtrasada = concluido < previsto;

        if (tarefaAtrasada)
        {
            if (tarefaMarco)
            {
                estilo = @"style=""atrasadaMarco""";
            }
            else
            {
                if (tarefaCritica)
                {
                    estilo = (tarefaSumario) ? @"style=""atrasadaCriticaSumario""" : @"style=""atrasadaCritica""";
                }
                else
                {
                    estilo = (tarefaSumario) ? @"style=""atrasadaSumario""" : @"style=""atrasada""";
                }
            }
        }
        else if (concluido > previsto)
        {
            if (tarefaMarco)
            {
                estilo = @"style=""adiantadaMarco""";
            }
            else
            {
                if (tarefaCritica)
                {
                    estilo = (tarefaSumario) ? @"style=""adiantadaCriticaSumario""" : @"style=""adiantadaCritica""";
                }
                else
                {
                    estilo = (tarefaSumario) ? @"style=""adiantadaSumario""" : @"style=""adiantada""";
                }
            }
        }
        else
        {
            if (tarefaCritica)
            {
                if (tarefaMarco)
                    estilo = @"style=""marcoCritico""";
                else
                    estilo = (tarefaSumario) ? @"style=""criticaSumario""" : @"style=""critica""";
            }
            else
            {
                if (tarefaMarco)
                {
                    estilo = @"style=""marco""";
                }
            }
        }
        return estilo;
    }

    private string getEstilosPA()
    {
        return string.Format(@"<styles>  
                          <defaults>
                              <task>
                                <task_style>
                                    <row_datagrid>
                                        <cell>
                                          <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                          
                                        </cell>
                                        <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                                      </row_datagrid>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                             <progress>
					          <bar_style>
					            <middle shape=""HalfCenter"" >
                                  <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" />					              
					              <border enabled=""true"" type=""Solid"" color=""Black"" opacity=""1"" />
					            </middle>
					          </bar_style>
					        </progress>			
                                </task_style>
                              </task>

                              <milestone>                                
                                <task_style>
                                <row_datagrid>
                            <cell>
                              <font face=""Verdana"" size=""11"" />     
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                       
                            </cell>
                            <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                          </row_datagrid>
                                <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                                    </task_style>
                              </milestone>
                              <summary>
                                <task_style>
                                  <row_datagrid>
                                    <cell>
                                      <font face=""Verdana"" size=""11"" />    
                                          <states>
                                         <selected_normal>
                                          <fill enabled=""true"" type=""Solid"" color=""#C3C3C3"" opacity=""0.5"" /> 
                                          </selected_normal>
                                          </states>                                                        
                                    </cell>
                                    <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  </row_datagrid>
                                  <tooltip enabled=""true"">
                                    <text>
                                    <![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}%
                                      ]]> 
                                      </text>
                                      </tooltip>
                                  <progress>
                                    <bar_style>
                                      <middle shape=""HalfTop"">
                                        <fill enabled=""true"" type=""Gradient"" color=""#CCDBFF"" opacity=""1"" />
                                      </middle>
                                    </bar_style>
                                  </progress>
                                </task_style>
                              </summary>
                            </defaults>                        
				          <task_styles>
                            <task_style name=""VERMELHO"">
				               <tooltip enabled=""true""><text><![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}% ]]></text></tooltip>
                              <row_datagrid><cell>
                              <font face=""Verdana"" size=""11"" color=""red"" />                                                        
                            </cell><tooltip enabled=""true""><text><![CDATA[Tarefa: {0}
Início: {1}
Término: {2}
Concluído: {3}% ]]></text></tooltip></row_datagrid>	
                                                      				               
				            </task_style>

                            <task_style name=""RVERMELHO"">
    				            <tooltip enabled=""true"">
                                <text>Plano de Ação: {0}
Início: {1}
Término: {2}
Concluído: {3}%                                    
                                </text>
                              </tooltip> 
                              <row_datagrid><cell>
                              <font face=""Verdana"" size=""11"" color=""red"" />                                                        
                            </cell>
                                <tooltip enabled=""true"">
                                <text>Plano de Ação: {0}
Início: {1}
Término: {2}
Concluído: {3}%</text> 
                              </tooltip>
                              </row_datagrid>  
                                <progress>
                                 <bar_style>					              
                                  <middle  shape=""HalfTop"">
						              <fill enabled=""true"" type=""Solid"" color=""#CCDBFF"" />
						            </middle>
                                </bar_style>
					        </progress>
                            <actual>		
                            <bar_style>
                                  <middle  shape=""HalfTop"">	
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>  
						            </middle>
                                  <start>
                                    <marker type=""Arrow"">
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />
                                    </marker>
                                  </start>
                                  <end>
                                    <marker type=""Arrow"">     
                                      <fill enabled=""true"" type=""Solid"" color=""#4A4A4A""/>
                                      <border color=""#454545"" />                                 
                                    </marker>
                                  </end>
                                </bar_style>
                             </actual>	                            			
				            </task_style>
				          </task_styles>
				        </styles>", "{%Name}", "{%ActualStart}{dateTimeFormat:%dd/%MM/%yyyy}", "{%ActualEnd}{dateTimeFormat:%dd/%MM/%yyyy}", "{%Complete}");
    }

    private string getStiloPlanoAcao(DataRow dr)
    {

        string statusPA = dr["Status"].ToString();
        int nivel = int.Parse(dr["Nivel"].ToString()) - 1;

        if (nivel == 0)
            statusPA = "R" + statusPA;

        return string.Format(@"style=""{0}""", statusPA.Trim().ToUpper());
    }

    public DataSet getMetasVisaoCorporativaProjetos(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"DECLARE
                    @Data DateTime
                  , @Ano SmallInt
                  , @Mes SmallInt

                  SET @Data = GETDATE()
                  SET @Ano = YEAR(@Data)
                  SET @Mes = MONTH(@Data)

                  SELECT mo.CodigoMetaOperacional
                   , mo.CodigoProjeto
                   , i.CodigoIndicador
	               , p.NomeProjeto
	               , mo.MetaDescritiva AS Meta
	               , rmo.ValorMetaAcumuladaAno AS MetaAcumuladaAno
	               , rmo.ValorResultadoAcumuladoAno AS ResultadoAcumuladoAno
	               , tum.SiglaUnidadeMedida
	               , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno, i.Polaridade, 'IO', mo.CodigoIndicador) AS Desempenho
                   , Polaridade
                   , i.CasasDecimais
                   , i.NomeIndicador
                   FROM {0}.{1}.IndicadorOperacional AS i INNER JOIN 
       	                {0}.{1}.MetaOperacional   AS mo ON (mo.CodigoIndicador = i.CodigoIndicador) INNER JOIN 
       	                {0}.{1}.Projeto AS p ON (p.CodigoProjeto = mo.CodigoProjeto)INNER JOIN 
                        {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto AND s.IndicaAcompanhamentoExecucao = 'S') INNER JOIN 
       	                {0}.{1}.TipoUnidadeMedida AS tum ON (tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida) LEFT JOIN 
       	                {0}.{1}.ResumoMetaOperacional AS rmo ON (rmo.CodigoMetaOperacional = mo.CodigoMetaOperacional
      							              AND rmo.Ano =  YEAR({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes))
      							              AND rmo.Mes = MONTH({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes)))
                  WHERE i.DataExclusao IS NULL  
                    AND mo.MetaDescritiva IS NOT NULL
                    AND p.CodigoEntidade = {2}
                    {3}
                  ORDER BY p.NomeProjeto
            ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getQTDMetasVisaoCorporativaProjetos(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"DECLARE
                    @Data DateTime
                  , @Ano SmallInt
                  , @Mes SmallInt

                  SET @Data = GETDATE()
                  SET @Ano = YEAR(@Data)
                  SET @Mes = MONTH(@Data)

                  SELECT COUNT(1) AS QTD
                   FROM {0}.{1}.IndicadorOperacional AS i INNER JOIN 
       	                {0}.{1}.MetaOperacional   AS mo ON (mo.CodigoIndicador = i.CodigoIndicador) INNER JOIN 
       	                {0}.{1}.Projeto AS p ON (p.CodigoProjeto = mo.CodigoProjeto)INNER JOIN 
                        {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto AND s.IndicaAcompanhamentoExecucao = 'S') INNER JOIN 
       	                {0}.{1}.TipoUnidadeMedida AS tum ON (tum.CodigoUnidadeMedida = i.CodigoUnidadeMedida) LEFT JOIN 
       	                {0}.{1}.ResumoMetaOperacional AS rmo ON (rmo.CodigoMetaOperacional = mo.CodigoMetaOperacional
      							              AND rmo.Ano =  YEAR({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes))
      							              AND rmo.Mes = MONTH({0}.{1}.f_GetDataRefUltimoDadoMetaOperacional(mo.CodigoMetaOperacional, @Ano, @Mes)))
                  WHERE i.DataExclusao IS NULL  
                    AND mo.MetaDescritiva IS NOT NULL
                    AND p.CodigoEntidade = {2}
                    {3}
            ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }


    public DataSet getMetasVisaoCorporativaIndicadores(int codigoEntidade, int codigoUsuario, string whereIndicadorObjetivo, string where)
    {
        string comandoSQL = string.Format(@"
            BEGIN
                /* Obtém a relação de todos os indicadores de um objetivo estratégico, mostrando as cores
                   de desempenho de cada um deles.
                */
                DECLARE @CorIndicador Varchar(8),
                        @CodigoIndicador Int,
                        @CodigoUnidade Int,
                        @NomeIndicador Varchar(255),
		                @Meta Varchar(2000),
                        @IndicadorResultante  char(1),
                        @AnoAtual SmallInt,
                        @MesAtual SmallInt,
                        @CasasDecimais SmallInt,
                        @NomeResponsavel VarChar(60),
                        @CodigoUsuarioResponsavel int,
                        @DataUltimaMensagem DateTime,
						@CodigoTipoAssociacao Int,
                        @Polaridade Char(3)
		                , @codigoUsuario                Int
		                , @vinculaMetasMapas			Char(1)
		                , @codigoEntidadeMapa			Int
		                , @codigoMapaContexto			Int


                DECLARE @tmp 
                TABLE (CodigoIndicador Int,
                       NomeIndicador Varchar(255),
                       Meta Varchar(2000),
                       Desempenho Varchar(8),
                       IndicadorResultante char(1),
                       CasasDecimais SmallInt,
                       NomeResponsavel VarChar(60),
                       CodigoUsuarioResponsavel int,
                       CodigoUltimaMensagem int,
                       Polaridade Char(3))

                SET @CodigoUnidade = {2}
                SET @codigoUsuario = {5}

                SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
                  FROM {0}.{1}.TipoAssociacao 
                 WHERE IniciaisTipoAssociacao = 'IN'
               
                SET @AnoAtual = Year(GetDate())
                SET @MesAtual = Month(GetDate())

	            SET @vinculaMetasMapas		= 'N';
	            SELECT @vinculaMetasMapas = CAST(pcs.[Valor] AS CHAR(1)) FROM {0}.{1}.[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'vinculaMetasEstrategicasAMapasEmPaineis'

	            SET @codigoMapaContexto = NULL;
	            IF ( @vinculaMetasMapas = 'S' )
	            BEGIN
		            SELECT @codigoMapaContexto = u.[CodigoMapaEstrategicoPadrao] FROM {0}.{1}.[Usuario] AS [u] WHERE u.[CodigoUsuario] = @codigoUsuario;
		
		            IF (@codigoMapaContexto IS NULL) BEGIN
			            SELECT @codigoMapaContexto = CAST(pcs.[Valor] AS Int) FROM [dbo].[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'CodigoMapaDefault' AND ISNUMERIC(pcs.[Valor]) = 1;
		            END -- IF (@codigoMapaContexto IS NULL) 
		            ELSE BEGIN
			            SET @codigoEntidadeMapa = dbo.f_GetCodigoEntidadeObjeto(@codigoMapaContexto, NULL, 'ME', 0);
			
			            IF ( (@codigoEntidadeMapa IS NULL) OR (@codigoEntidadeMapa != @CodigoUnidade) ) BEGIN
				            SET @codigoMapaContexto = NULL;
				            SELECT @codigoMapaContexto = CAST(pcs.[Valor] AS Int) FROM [dbo].[ParametroConfiguracaoSistema] AS [pcs] WHERE pcs.[CodigoEntidade] = @CodigoUnidade AND pcs.[Parametro] = 'CodigoMapaDefault' AND ISNUMERIC(pcs.[Valor]) = 1;
			            END -- IF ( (@codigoEntidadeMapa IS NULL) OR ...
		            END -- ELSE (@codigoMapaContexto IS NULL) 
	            END -- IF ( @vinculaMetasMapas = 'S' )


	            IF (@codigoMapaContexto IS NULL) BEGIN

                    DECLARE cCursorIndicador 
                    CURSOR FOR SELECT DISTINCT
                                      i.CodigoIndicador,
                                      i.NomeIndicador,
                                      iu.Meta,
                                      i.IndicadorResultante,
                                      i.CasasDecimais,
                                      u.NomeUsuario,
                                      iu.CodigoResponsavelIndicadorUnidade,
                                      i.Polaridade
                                 FROM {0}.{1}.Indicador i 
                                INNER JOIN {0}.{1}.[IndicadorUnidade]	AS [iu] ON (    iu.[CodigoIndicador]        = i.[CodigoIndicador] 
                                                                                    AND iu.[CodigoUnidadeNegocio]	= @CodigoUnidade
                                                                                    AND iu.Meta IS NOT NULL)
                                LEFT JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade
                                WHERE i.[DataExclusao]			    IS NULL 
                                  AND iu.[DataExclusao]				IS NULL
				                  AND i.[CodigoIndicador] IN ( SELECT f.[CodigoIndicador] FROM  dbo.f_GetIndicadoresUsuario(@CodigoUsuario, @CodigoUnidade, 'N') f )
                                  {4}

	            END -- IF (@codigoMapaContexto IS NULL) 
                ELSE BEGIN -- 

                    DECLARE cCursorIndicador 
                    CURSOR FOR SELECT DISTINCT
                                      i.CodigoIndicador,
                                      i.NomeIndicador,
                                      iu.Meta,
                                      i.IndicadorResultante,
                                      i.CasasDecimais,
                                      u.NomeUsuario,
                                      iu.CodigoResponsavelIndicadorUnidade,
                                      i.Polaridade
				                FROM 
					                {0}.{1}.[Indicador]												AS [i] 
					
					                INNER JOIN {0}.{1}.[IndicadorUnidade]			AS [iu]		ON 
						                (			iu.[CodigoIndicador]	= i.[CodigoIndicador] 
							                AND iu.[CodigoUnidadeNegocio]	= @CodigoUnidade
							                AND iu.[DataExclusao]			IS NULL
							                AND iu.[Meta]					IS NOT NULL		)
							
					                INNER JOIN {0}.{1}.[IndicadorObjetivoEstrategico]	AS [ioe]		ON 
						                (	 ioe.[CodigoIndicador]	= i.[CodigoIndicador] )
						
					                INNER JOIN {0}.{1}.[ObjetoEstrategia]			AS [oe]		ON 
						                (			oe.[CodigoObjetoEstrategia]	= ioe.[CodigoObjetivoEstrategico]
							                AND oe.[CodigoMapaEstrategico]	    = @codigoMapaContexto
							                AND oe.[DataExclusao]				IS NULL     )
							
					                INNER JOIN {0}.{1}.[TipoObjetoEstrategia]	AS [toe]	ON 
						                (			toe.[CodigoTipoObjetoEstrategia]	= oe.[CodigoTipoObjetoEstrategia]
							                AND toe.[IniciaisTipoObjeto]				= 'OBJ')
					
					                INNER JOIN {0}.{1}.[MapaEstrategico]			AS [me]		ON 
						                (			me.[CodigoMapaEstrategico]	= oe.[CodigoMapaEstrategico]
							                AND me.[IndicaMapaEstrategicoAtivo]	= 'S'  )
					
                                    LEFT JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade
                                
			                    WHERE i.[DataExclusao]					IS NULL 
				                    AND i.[CodigoIndicador] IN ( SELECT f.[CodigoIndicador] FROM  dbo.f_GetIndicadoresUsuario(@codigoUsuario, @CodigoUnidade, 'N') f )
                                    {4}
                END -- ELSE (@codigoMapaContexto IS NULL) 
                              

                OPEN cCursorIndicador

                FETCH NEXT FROM cCursorIndicador INTO @CodigoIndicador,
                                                      @NomeIndicador,
                                                      @Meta,
                                                      @IndicadorResultante,
                                                      @CasasDecimais,
                                                      @NomeResponsavel,
                                                      @CodigoUsuarioResponsavel,
                                                      @Polaridade

                WHILE @@FETCH_STATUS = 0
                  BEGIN
                    
                    SET @CorIndicador = {0}.{1}.f_GetUltimoDesempenhoIndicador(@CodigoUnidade, @CodigoIndicador, @AnoAtual, @MesAtual, 'A')

                    INSERT INTO @tmp
                      (CodigoIndicador,
                       NomeIndicador,
                       Meta,
                       Desempenho,
                       IndicadorResultante,
                       CasasDecimais,
                       NomeResponsavel,
                       CodigoUsuarioResponsavel,
                       Polaridade)
                    VALUES
                      (@CodigoIndicador,
                       @NomeIndicador,
                       @Meta,
                       @CorIndicador,
                       @IndicadorResultante,
                       @CasasDecimais,
                       @NomeResponsavel,
                       @CodigoUsuarioResponsavel,
                       @Polaridade)

                    FETCH NEXT FROM cCursorIndicador INTO @CodigoIndicador,
			                                              @NomeIndicador,
			                                              @Meta,
                                                          @IndicadorResultante,
                                                          @CasasDecimais,
                                                          @NomeResponsavel,
                                                          @CodigoUsuarioResponsavel,
                                                          @Polaridade
                  END

                CLOSE cCursorIndicador

                DEALLOCATE cCursorIndicador
                
                UPDATE @tmp 
                   SET CodigoUltimaMensagem = (SELECT Max(CodigoMensagem)
											   FROM {0}.{1}.Mensagem 
											  WHERE CodigoTipoAssociacao = @CodigoTipoAssociacao
											    AND CodigoObjetoAssociado = CodigoIndicador
											    AND DataResposta IS NULL
											    AND CodigoEntidade = @CodigoUnidade)

                SELECT *, 
					   ISNULL((SELECT CASE WHEN m.DataLimiteResposta IS NULL THEN 'SM' 
							WHEN m.DataLimiteResposta + 1 > GETDATE() THEN 'MN'
							ELSE 'MA' END TipoMensagem 
					     FROM {0}.{1}.Mensagem m 
						WHERE m.CodigoMensagem = tmp.CodigoUltimaMensagem), 'SM') AS TipoMensagem
                  FROM @tmp tmp
                 WHERE 1 = 1
                    {3}
                 ORDER BY Meta
            END
                           
            ", bancodb, Ownerdb, codigoEntidade, where, whereIndicadorObjetivo, codigoUsuario);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaPainelMetas(int codigoEntidade, int codigoUsuario, string whereIndicadorObjetivo, int codigoMapa, int codigoUnidade, string where)
    {
        string whereUnidade = "";

        if (codigoUnidade != -1)
            whereUnidade = "AND iu.[CodigoUnidadeNegocio]	= " + codigoUnidade;

        string comandoSQL = string.Format(@"
BEGIN
	DECLARE 
			@CorIndicador									Varchar(8)
		,	@CodigoIndicador							Int
		,	@CodigoEntidade								Int
		,	@NomeIndicador								Varchar(255)
		,	@Meta													Varchar(2000)
		,	@IndicadorResultante					char(1)
		,	@AnoAtual											SmallInt
		,	@MesAtual											SmallInt
		,	@CasasDecimais								SmallInt
		,	@NomeResponsavel							VarChar(60)
		,	@CodigoUsuarioResponsavel			Int
		,	@DataUltimaMensagem						DateTime
		,	@CodigoTipoAssociacao					Int
		,	@Polaridade										Char(3)
        , @codigoUsuario                Int
        , @vinculaMetasMapas						Char(1)
        , @codigoEntidadeMapa						Int
        , @codigoMapaContexto						Int
		, @NomeUnidadeNegocio           VARCHAR(500)
		, @CodigoUnidadeNegocio					Int

	DECLARE @tmp TABLE 
		(		CodigoIndicador							Int
			,	NomeIndicador								Varchar(255)
			,	Meta												Varchar(2000)
			,	Desempenho									Varchar(8)
			,	IndicadorResultante					Char(1)
			,	CasasDecimais								SmallInt
			,	NomeResponsavel							Varchar(60)
			,	CodigoUsuarioResponsavel		Int
			,	CodigoUltimaMensagem				Int
			,	Polaridade									Char(3)
			,	NomeUnidadeNegocio					Varchar(500)
            ,   CodigoUnidadeNegocio                Int
		)

	DECLARE @tbResumo TABLE 
		(
				[CodigoIndicador]													Int
			, [IndicaUnidadeCriadoraIndicador]	        Char(1)
			, [CodigoResponsavelIndicadorUnidade]				Int
			, [CodigoResponsavelAtualizacaoIndicador]   Int
			, [CodigoUnidadeNegocio]                    Int
		)

	SET @CodigoEntidade = {2};
	SET @codigoUsuario	= {5};

	SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao
		FROM {0}.{1}.TipoAssociacao 
		WHERE IniciaisTipoAssociacao = 'IN'

	SET @AnoAtual = Year(GetDate());
	SET @MesAtual = Month(GetDate());

	SET @codigoMapaContexto = {6};

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados na ENTIDADE atual
					i.[CodigoIndicador]
				, 'S'
				, iu.[CodigoResponsavelIndicadorUnidade]
				, iu.[CodigoResponsavelAtualizacaoIndicador]
				, iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]

				INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]				= i.[CodigoIndicador]
						AND iu.[CodigoUnidadeNegocio]		= @CodigoEntidade
						AND iu.[DataExclusao]						IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] = 'S'
					)
			WHERE   i.[DataExclusao]          IS NULL
		UNION ALL
		SELECT -- dos indicadores criados em UNIDADE da ENTIDADE atual
					i.[CodigoIndicador]
				, 'S'
				, iu.[CodigoResponsavelIndicadorUnidade]
				, iu.[CodigoResponsavelAtualizacaoIndicador]
				, iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]

				INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]								= i.[CodigoIndicador]
						AND iu.[DataExclusao]										IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador] = 'S'
					)

				INNER JOIN {0}.{1}.[UnidadeNegocio]			AS [un]		ON 
					(			un.[CodigoUnidadeNegocio]				= iu.[CodigoUnidadeNegocio]
						AND un.DataExclusao									IS NULL
						AND un.[IndicaUnidadeNegocioAtiva]	= 'S'
						AND un.[CodigoEntidade]							= @CodigoEntidade
					)
			WHERE   i.[DataExclusao]          IS NULL
				AND NOT EXISTS
					( SELECT TOP 1 1 
							FROM {0}.{1}.[IndicadorUnidade]				AS [iu2]
							WHERE iu2.CodigoIndicador	                    = i.CodigoIndicador
								AND iu2.DataExclusao												IS NULL
								AND iu2.[CodigoUnidadeNegocio]							= @CodigoEntidade
								AND iu2.[IndicaUnidadeCriadoraIndicador]		= 'S'
					);

	INSERT INTO @tbResumo			
		SELECT -- dos indicadores criados em outra entidade, mas compartilhado com a entidade atual
					i.[CodigoIndicador]
				, 'N'
				, iu.[CodigoResponsavelIndicadorUnidade]
				, iu.[CodigoResponsavelAtualizacaoIndicador]
				, iu.[CodigoUnidadeNegocio]
			FROM
				{0}.{1}.[Indicador]					AS [i]

				INNER JOIN {0}.{1}.[IndicadorUnidade]		AS [iu]		ON 
					(			iu.[CodigoIndicador]									= i.[CodigoIndicador]
						AND iu.[CodigoUnidadeNegocio]							= @CodigoEntidade
						AND iu.[DataExclusao]											IS NULL
						AND iu.[IndicaUnidadeCriadoraIndicador]		!= 'S'
					)
			WHERE   i.[DataExclusao]  IS NULL
				AND i.[CodigoIndicador] NOT IN ( SELECT [CodigoIndicador] FROM @tbResumo )
	            
	IF (@codigoMapaContexto = -1) BEGIN

    DECLARE cCursorIndicador CURSOR FOR 
			SELECT DISTINCT
					i.CodigoIndicador,
					i.NomeIndicador,
					iu.Meta,
					i.IndicadorResultante,
					i.CasasDecimais,
					u.NomeUsuario,
					iu.CodigoResponsavelIndicadorUnidade,
					i.Polaridade,
					un.NomeUnidadeNegocio,
					tmp.CodigoUnidadeNegocio
				FROM @tbResumo         AS [tmp]

					INNER JOIN {0}.{1}.[Indicador]	AS [i] ON (i.[CodigoIndicador] = tmp.[CodigoIndicador])
					
					INNER JOIN {0}.{1}.[IndicadorUnidade]	AS [iu]     ON 
							(			iu.[CodigoIndicador]        = tmp.[CodigoIndicador] 
								AND iu.[CodigoUnidadeNegocio]   = tmp.[CodigoUnidadeNegocio]
								{7}
								AND iu.[Meta] IS NOT NULL 
							)
					
					INNER JOIN {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
					LEFT JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade
				WHERE i.[DataExclusao]		IS NULL 
					AND iu.[DataExclusao]		IS NULL
          AND i.[CodigoIndicador] IN ( SELECT f.[CodigoIndicador] FROM  {0}.{1}.f_GetIndicadoresUsuario(@CodigoUsuario, @CodigoEntidade, 'N') f )
					AND un.DataExclusao IS NULL
					AND un.CodigoEntidade = @CodigoEntidade
					{4}
	END -- IF (@codigoMapaContexto IS NULL) 
	ELSE BEGIN -- 

    DECLARE cCursorIndicador CURSOR FOR 
			SELECT DISTINCT
					i.CodigoIndicador,
					i.NomeIndicador,
					iu.Meta,
					i.IndicadorResultante,
					i.CasasDecimais,
					u.NomeUsuario,
					iu.CodigoResponsavelIndicadorUnidade,
					i.Polaridade,
					un.NomeUnidadeNegocio,
					tmp.CodigoUnidadeNegocio
				FROM @tbResumo         AS [tmp]
					
					INNER JOIN {0}.{1}.[Indicador]										AS [i]		ON 
						(i.[CodigoIndicador] = tmp.[CodigoIndicador])
					
					INNER JOIN {0}.{1}.[IndicadorUnidade]							AS [iu]		ON 
						(			iu.[CodigoIndicador]        = tmp.[CodigoIndicador] 
							AND iu.[CodigoUnidadeNegocio]    = tmp.[CodigoUnidadeNegocio]
							{7}
							AND iu.[Meta] IS NOT NULL 
						)

          INNER JOIN {0}.{1}.[IndicadorObjetivoEstrategico]	AS [ioe]	ON 
            (	 ioe.[CodigoIndicador]	= i.[CodigoIndicador] )

          INNER JOIN {0}.{1}.[ObjetoEstrategia]							AS [oe]		ON 
            (			oe.[CodigoObjetoEstrategia]	= ioe.[CodigoObjetivoEstrategico]
              AND oe.[CodigoMapaEstrategico]	= @codigoMapaContexto
              AND oe.[DataExclusao]						IS NULL 
            )

          INNER JOIN {0}.{1}.[TipoObjetoEstrategia]					AS [toe]	ON 
            (			toe.[CodigoTipoObjetoEstrategia]	= oe.[CodigoTipoObjetoEstrategia]
              AND toe.[IniciaisTipoObjeto]					= 'OBJ'
            )

          INNER JOIN {0}.{1}.[MapaEstrategico]			AS [me]		ON 
            (			me.[CodigoMapaEstrategico]			= oe.[CodigoMapaEstrategico]
              AND me.[IndicaMapaEstrategicoAtivo]	= 'S'  
            )

					LEFT JOIN {0}.{1}.Usuario u ON u.CodigoUsuario = iu.CodigoResponsavelIndicadorUnidade
					INNER JOIN {0}.{1}.UnidadeNegocio un ON un.CodigoUnidadeNegocio = iu.CodigoUnidadeNegocio
					
				WHERE i.[DataExclusao]					IS NULL 
					AND i.[CodigoIndicador] IN (SELECT f.[CodigoIndicador] FROM  {0}.{1}.f_GetIndicadoresUsuario(@codigoUsuario, @CodigoEntidade, 'N') f )
					{4}
                    
	END -- ELSE (@codigoMapaContexto IS NULL) 
  OPEN cCursorIndicador;

	FETCH NEXT FROM cCursorIndicador INTO 
			@CodigoIndicador
		,	@NomeIndicador
		,	@Meta
		,	@IndicadorResultante
		,	@CasasDecimais
		,	@NomeResponsavel
		,	@CodigoUsuarioResponsavel
		,	@Polaridade
		,	@NomeUnidadeNegocio
		,	@CodigoUnidadeNegocio

	WHILE (@@FETCH_STATUS = 0) BEGIN
    
    SET @CorIndicador = {0}.{1}.f_GetUltimoDesempenhoIndicador(@CodigoUnidadeNegocio, @CodigoIndicador, @AnoAtual, @MesAtual, 'A')

    INSERT INTO @tmp
			(		CodigoIndicador							,	NomeIndicador											,	Meta
				,	Desempenho									,	IndicadorResultante								,	CasasDecimais
				,	NomeResponsavel							,	CodigoUsuarioResponsavel					,	Polaridade
				,	NomeUnidadeNegocio                      ,   CodigoUnidadeNegocio
			)
			VALUES
      (		@CodigoIndicador						,	@NomeIndicador										,	@Meta
				,	@CorIndicador								,	@IndicadorResultante							,	@CasasDecimais
				,	@NomeResponsavel						,	@CodigoUsuarioResponsavel					,	@Polaridade
				,	@NomeUnidadeNegocio                     ,   @CodigoUnidadeNegocio
			)

		FETCH NEXT FROM cCursorIndicador INTO 
				@CodigoIndicador
			,	@NomeIndicador
			,	@Meta
			,	@IndicadorResultante
			,	@CasasDecimais
			,	@NomeResponsavel
			,	@CodigoUsuarioResponsavel
			,	@Polaridade
			,	@NomeUnidadeNegocio
			,	@CodigoUnidadeNegocio
  END

	CLOSE cCursorIndicador
	DEALLOCATE cCursorIndicador
                
	UPDATE @tmp 
		SET 
			CodigoUltimaMensagem = 
				(	SELECT Max(CodigoMensagem)
						FROM {0}.{1}.Mensagem 
						WHERE CodigoTipoAssociacao	= @CodigoTipoAssociacao
							AND CodigoObjetoAssociado = CodigoIndicador
							AND DataResposta					IS NULL
							AND CodigoEntidade				= @CodigoEntidade
				);

    DELETE @tmp
     FROM @tmp AS tmp
    WHERE EXISTS(SELECT 1 
                   FROM {0}.{1}.Indicador AS i 
                  WHERE i.CodigoIndicador = tmp.CodigoIndicador
                    AND GETDATE() NOT BETWEEN IsNull(i.DataInicioValidadeMeta,'01/01/1900') AND IsNull(i.DataTerminoValidadeMeta,'31/12/2999')
                    AND i.IndicaAcompanhamentoMetaVigencia = 'S')

	SELECT *
			, ISNULL(
				(	SELECT 
							CASE WHEN m.DataLimiteResposta IS NULL THEN 'SM' 
											WHEN m.DataLimiteResposta + 1 > GETDATE() THEN 'MN'
											ELSE 'MA' 
							END TipoMensagem 
						FROM {0}.{1}.Mensagem m 
						WHERE m.CodigoMensagem = tmp.CodigoUltimaMensagem), 'SM'
				) AS TipoMensagem
		FROM @tmp tmp
		WHERE 1 = 1
			{3}
		ORDER BY Meta;
END ", bancodb, Ownerdb, codigoEntidade, where, whereIndicadorObjetivo, codigoUsuario, codigoMapa, whereUnidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getMetaProjetoIndicadorVisaoCorporativa(int codigoMeta, string where)
    {
        string comandoSQL = string.Format(
              @"BEGIN
	            DECLARE @CodigoUsuarioInclusao INT
                      , @CodigoMeta INT
                      , @TipoAssociacao Int

	            DECLARE @tbResumo TABLE 
                 (  Periodo VarChar(50),
					ValorRealizado decimal(18,4),
					ValorPrevisto decimal(18,4),
                    CorIndicador VarChar(10),
					Ano int,
					mes int
                 )
				SET @CodigoMeta	= {2}
				SET @TipoAssociacao	= {0}.{1}.f_GetCodigoTipoAssociacao('MT')
	            													
	            INSERT INTO @tbResumo
                      SELECT {0}.{1}.f_GetNomePeriodoMetaOperacional(rmo.CodigoMetaOperacional, rmo.Ano, rmo.Mes, 1, 2)
                           , rmo.ValorResultadoAcumuladoAno
                           , rmo.ValorMetaAcumuladaAno
                           , {0}.{1}.f_GetCorDesempenhoResultado(rmo.ValorMetaAcumuladaAno, rmo.ValorResultadoAcumuladoAno, i.Polaridade,'IO',mo.CodigoIndicador) AS Desempenho
                           , rmo.Ano
                           , rmo.Mes
                       FROM {0}.{1}.ResumoMetaOperacional AS rmo INNER JOIN 
							{0}.{1}.MetaOperacional mo ON mo.CodigoMetaOperacional = rmo.CodigoMetaOperacional  INNER JOIN 
			                {0}.{1}.IndicadorOperacional AS i ON (i.CodigoIndicador = mo.CodigoIndicador)
                      WHERE rmo.CodigoMetaOperacional = @CodigoMeta
                      ORDER BY rmo.Ano
                             , rmo.Mes
	            SELECT      Periodo
                        ,   ValorRealizado
                        ,   ValorPrevisto
                        ,   CorIndicador
                        ,   Ano
                        ,   mes
                        ,   (SELECT     CodigoAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS CodigoAnalisePerformance
                        ,   (SELECT     DataAnalisePerformance 
                                FROM    {0}.{1}.AnalisePerformance AS ap 
                                WHERE   ap.DataExclusao     IS NULL
									AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS DataAnalisePerformance
                        ,   (SELECT     Recomendacoes 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS Recomendacoes
                        ,   (SELECT     Analise 
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS Analise
                        ,   (SELECT     DataInclusao
                                FROM    {0}.{1}.AnalisePerformance ap 
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS DataInclusao
                        ,   (SELECT     CodigoUsuarioInclusao
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS CodigoUsuarioInclusao
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ap.CodigoUsuarioInclusao = u.CodigoUsuario)
                                WHERE ap.DataExclusao       IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS NomeUsuario
                        ,   (SELECT     u.NomeUsuario
                                FROM        {0}.{1}.AnalisePerformance  AS ap 
                                INNER JOIN  {0}.{1}.Usuario             AS u ON (ISNULL(ap.CodigoUsuarioUltimaAlteracao, ap.CodigoUsuarioInclusao) = u.CodigoUsuario)
                                WHERE   ap.DataExclusao IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano = tb.Ano
                                    AND ap.Mes = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS NomeUsuarioUltimaAlteracao
                       ,      (SELECT      ISNULL(DataUltimaAlteracao, DataInclusao)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS DataUltimaAlteracao
                        ,   (SELECT     CONVERT(Char(10), ISNULL(DataUltimaAlteracao, DataInclusao), 103)
                                FROM    {0}.{1}.AnalisePerformance AS ap
                                WHERE   ap.DataExclusao     IS NULL
                                    AND ap.[IndicaTipoIndicador] = 'O'
                                    AND ap.Ano              = tb.Ano
                                    AND ap.Mes              = tb.Mes
                                    AND ap.CodigoObjetoAssociado = @CodigoMeta
                                    AND ap.CodigoTipoAssociacao = @TipoAssociacao) AS DataUltimaAlteracaoFormatada
                        ,   UPPER( {0}.{1}.f_GetNomePeriodoMetaOperacional (@CodigoMeta, tb.Ano, tb.Mes,1,2)) AS DetalhamentoPeriodo
	              FROM @tbResumo tb 
                 WHERE 1 = 1
                   {3} 
            END
            ", bancodb, Ownerdb, codigoMeta, where);
        return getDataSet(comandoSQL);
    }

    #endregion
}
}