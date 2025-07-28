    using System;
using System.Collections;
using System.Data;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region _Estrategia - Wizard - menuMapa - TemasMapa.aspx

    public bool incluiTemasMapa(int codigoMapaEstrategico, int codigoVersaoMapa, string iniciaisTipoObjeto
                                    , string TituloObjetoEstrategia, string DescricaoObjetoEstrategia, string glossario, int idUsuarioInclusao
                                    , int idResponsavelObjeto, int idObjetoSuperior
                                    , ref string codigoNovoObjetivoEstrategico, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{5}' AND CodigoMapaEstrategico = {2} AND DataExclusao IS NULL)
                BEGIN
                    SELECT -1 AS Codigo, 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    DECLARE     @CodigoObjetoEstrategia      INT
                            ,   @CodigoMapaEstrategico          INT
                            ,   @CodigoVersaoMapaEstrategico    INT
                            ,   @CodigoTipoObjeto               INT

                    SET @CodigoMapaEstrategico          = {2}
                    SET @CodigoVersaoMapaEstrategico    = {3}
                    SELECT @CodigoTipoObjeto = CodigoTipoObjetoEstrategia FROM {0}.{1}.TipoObjetoEstrategia WHERE IniciaisTipoObjeto = '{4}'

                    INSERT INTO {0}.{1}.ObjetoEstrategia( 
                                oe.CodigoMapaEstrategico
                            ,   oe.CodigoVersaoMapaEstrategico
                            ,   TituloObjetoEstrategia
                            ,   GlossarioObjeto
                            ,   CodigoTipoObjetoEstrategia
                            ,   AlturaObjetoEstrategia
                            ,   LarguraObjetoEstrategia 
                            ,   TopoObjetoEstrategia
                            ,   EsquerdaObjetoEstrategia
                            ,   CorFundoObjetoEstrategia
                            ,   CorBordaObjetoEstrategia
                            ,   CorFonteObjetoEstrategia
                            ,   OrdemObjeto
                            ,   CodigoUsuarioInclusao
                            ,   DataInclusao
                            ,   CodigoResponsavelObjeto
                            ,   CodigoObjetoEstrategiaSuperior
                            ,   DescricaoObjetoEstrategia)
                    VALUES (
                            @CodigoMapaEstrategico
                        ,   @CodigoVersaoMapaEstrategico
                        ,   '{5}'
                        ,   {6}
                        ,   @CodigoTipoObjeto
                        ,   70
                        ,   486
                        ,   84
                        ,   11
                        ,   '#FFFFFF'
                        ,   '#800000'
                        ,   '#000000'
                        ,   2
                        ,   {7}
                        ,   GETDATE()
                        ,   {8}
                        ,   {9}
                        ,  '{10}')

                    SET @CodigoObjetoEstrategia = SCOPE_IDENTITY()

                    --Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                    DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'TM'       
                    SET @in_codigoObjeto = @CodigoObjetoEstrategia
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {11}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {7}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL

                    SELECT @CodigoObjetoEstrategia AS Codigo, 'OK' AS Resultado
                END
            ", bancodb, Ownerdb, codigoMapaEstrategico, codigoVersaoMapa, iniciaisTipoObjeto
             , TituloObjetoEstrategia
             , glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL"
             , idUsuarioInclusao
             , idResponsavelObjeto != -1 ? idResponsavelObjeto.ToString() : "NULL"
             , idObjetoSuperior, DescricaoObjetoEstrategia, codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            codigoNovoObjetivoEstrategico = ds.Tables[0].Rows[0]["Codigo"].ToString();
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTemasMapa(string tituloObjeto, string descricaoObjetoEstrategia, int idUsuarioEdicao, string glossario
                                        , int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa
                                        , int idResponsavel, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{2}' AND CodigoObjetoEstrategia != {5} AND CodigoMapaEstrategico = {6} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     TituloObjetoEstrategia          = '{2}'
                        ,   GlossarioObjeto                 = {3}
                        ,   CodigoUsuarioUltimaAlteracao    = {4}
                        ,   DataUltimaAlteracao             = GETDATE()
                        ,   CodigoResponsavelObjeto         = {8}
                        ,   DescricaoObjetoEstrategia       ='{9}'

                    WHERE   CodigoObjetoEstrategia      = {5}
                        AND CodigoMapaEstrategico       = {6}
                        AND CodigoVersaoMapaEstrategico = {7}

--Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'TM'       
                    SET @in_codigoObjeto = {5}
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {10}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {4}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL

                    SELECT 'OK' AS Resultado
                END
                ", bancodb, Ownerdb, tituloObjeto
                 , (glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL")
                 , idUsuarioEdicao, idObjetoEstrategia
                 , idMapaEstrategia, idVersaoMapa
                 , idResponsavel != -1 ? idResponsavel.ToString() : "NULL"
                 , descricaoObjetoEstrategia
                 , codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiTemasMapa(int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa, int idUsuarioExclusao
                                    , ref string resultado)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia where CodigoObjetoEstrategiaSuperior = {3} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     CodigoUsuarioExclusao = {2}
                        ,   DataExclusao = GETDATE()

                    WHERE   CodigoObjetoEstrategia      = {3}
                        AND CodigoMapaEstrategico       = {4}
                        AND CodigoVersaoMapaEstrategico = {5}
                    
                    SELECT 'OK' AS Resultado

                END
                ", bancodb, Ownerdb, idUsuarioExclusao
                 , idObjetoEstrategia, idMapaEstrategia, idVersaoMapa);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region _Estrategia - Wizard - menuMapa - ObjetivosEstrategicosMapa.aspx

    /// <summary>
    /// Função para retornar um 'result set' dos objetos de estratégias
    /// </summary>
    /// <remarks>A função retorna um result set dos objetos de estratégias. Apenas objetos PSP, TEM e OBJ são 
    /// retornados.
    /// </remarks>
    /// <param name="codigoUsuario">Código do usuário para verificação de quais objetos deverão ser listados</param>
    /// <param name="codigoEntidade">Código da entidade de origem dos mapas. Somente mapas criadas na entidade parâmetro serão listados</param>
    /// <param name="where">String com cláusulas where adicionais. (deverá iniciar com o operador AND ...</param>
    /// <param name="orderBy">String com a cláusular ORDER BY, se desejada alguma ordem. (deverá iniciar com ORDER BY ...</param>
    /// <param name="bColunaPermissoes">Boleano indicando se deseja a coluna [Permissoes] no select</param>
    /// <param name="iniciaisTipoAssociacao">Iniciais do tipo de associação do objeto em que serão verificadas 
    ///  as permissões caso se dejese que a coluna [Permissoes] faça parte do result set.</param>
    /// <returns>Retorna um data set da tabela [dbo].[ObjetoEstrategia].</returns>
    public DataSet getObjetosEstrategicos(int codigoUsuario, int codigoEntidade, string where, string orderBy, bool bColunaPermissoes, string iniciaisTipoAssociacao)
    {
        string comandoSQL = @"
            SELECT me.[TituloMapaEstrategico]
            ,   CASE    WHEN    (oe.[TituloObjetoEstrategia] IS NULL OR oe.[TituloObjetoEstrategia] = '')
                        THEN    oe.[DescricaoObjetoEstrategia]
                        ELSE    oe.[TituloObjetoEstrategia]
                        END     AS [DescricaoObjeto]
            ,   CASE toe.[IniciaisTipoObjeto]
                    WHEN 'TEM' THEN '../../../imagens/mapaEstrategico/TemaCombo.png' 
                    WHEN 'PSP' THEN '../../../imagens/mapaEstrategico/PerspectivaCombo.png' 
                    ELSE NULL   END     AS [urlImagemObjetoCombo]
            , oe.*, us.NomeUsuario as [ResponsavelObjeto] 
            , oe.[TituloObjetoEstrategia]
            , oe.[DescricaoObjetoEstrategia]
            , oe.ClassificacaoObjetoEstrategia AS ClassificacaoObjetivo
            , oe.InicioValidade
            , oe.TerminoValidade";

        if (bColunaPermissoes == true)
        {
            if (iniciaisTipoAssociacao == "")
            {
                comandoSQL += @"
                , CAST( 0 AS Int) AS [Permissoes] ";
            }
            else
            {

                comandoSQL += string.Format(@"
                ,   {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, oe.[CodigoObjetoEstrategia], NULL, '{4}', 0, NULL, '{5}')               * 2 +
                    {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, oe.[CodigoMapaEstrategico] , NULL, 'ME' , 0, NULL, 'ME_ExcPspTemObj')   * 4 +
                    {0}.{1}.f_VerificaAcessoConcedido({2}, {3}, oe.[CodigoObjetoEstrategia], NULL, '{4}', 0, NULL, '{6}')               * 8 AS [Permissoes]
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, iniciaisTipoAssociacao, iniciaisTipoAssociacao + "_Alt", iniciaisTipoAssociacao + "_AdmPrs");
            }
        }

        comandoSQL += string.Format(@"
            FROM        {0}.{1}.[ObjetoEstrategia]      AS [oe]
            INNER JOIN  {0}.{1}.[TipoObjetoEstrategia]  AS [toe]  ON oe.[CodigoTipoObjetoEstrategia]    = toe.[CodigoTipoObjetoEstrategia]
            INNER JOIN  {0}.{1}.[MapaEstrategico]       AS [me]   ON oe.[CodigoMapaEstrategico]         = me.[CodigoMapaEstrategico]
            INNER JOIN  {0}.{1}.[UnidadeNegocio]        AS [un]   ON un.[CodigoUnidadeNegocio]          = me.[CodigoUnidadeNegocio]
            left JOIN   {0}.{1}.[Usuario]               as [us]   on oe.CodigoResponsavelObjeto = us.CodigoUsuario
            WHERE   oe.[DataExclusao]                   IS NULL
                AND un.[CodigoEntidade]                 = {2} 
                AND un.[DataExclusao]                   IS NULL
                {3}
                {4}
            ", bancodb, Ownerdb, codigoEntidade, where, orderBy);
        try
        {
            DataSet ds = getDataSet(comandoSQL);
            return ds;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public bool incluiObjetivosMapa(int codigoMapaEstrategico, int codigoVersaoMapa, string iniciaisTipoObjeto
                                , string TituloObjetoEstrategia, string descricaoObjetoEstrategia, string glossario, int idUsuarioInclusao
                                , int idResponsavelObjeto, int idObjetoSuperior, string classificacaoObjetivo, string inicioValidade, string terminoValidade
                                , ref string codigoNovoObjetivoEstrategico, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{5}' AND CodigoMapaEstrategico = {2} AND DataExclusao IS NULL)
                BEGIN
                    SELECT -1 AS Codigo, 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    DECLARE     @CodigoObjetivoEstrategico      INT
                            ,   @CodigoMapaEstrategico          INT
                            ,   @CodigoVersaoMapaEstrategico    INT
                            ,   @CodigoTipoObjeto               INT

                    SET @CodigoMapaEstrategico          = {2}
                    SET @CodigoVersaoMapaEstrategico    = {3}
                    SELECT @CodigoTipoObjeto = CodigoTipoObjetoEstrategia FROM {0}.{1}.TipoObjetoEstrategia WHERE IniciaisTipoObjeto = '{4}'

                    INSERT INTO {0}.{1}.ObjetoEstrategia( 
                                oe.CodigoMapaEstrategico
                            ,   oe.CodigoVersaoMapaEstrategico
                            ,   DescricaoObjetoEstrategia
                            ,   GlossarioObjeto
                            ,   CodigoTipoObjetoEstrategia
                            ,   AlturaObjetoEstrategia
                            ,   LarguraObjetoEstrategia 
                            ,   TopoObjetoEstrategia
                            ,   EsquerdaObjetoEstrategia
                            ,   CorFundoObjetoEstrategia
                            ,   CorBordaObjetoEstrategia
                            ,   CorFonteObjetoEstrategia
                            ,   OrdemObjeto
                            ,   CodigoUsuarioInclusao
                            ,   DataInclusao
                            ,   CodigoResponsavelObjeto
                            ,   CodigoObjetoEstrategiaSuperior
                            ,   tituloObjetoEstrategia
                            ,   ClassificacaoObjetoEstrategia
                            ,   InicioValidade
                            ,   TerminoValidade)
                    VALUES (
                            @CodigoMapaEstrategico
                        ,   @CodigoVersaoMapaEstrategico
                        ,   '{5}'
                        ,   {6}
                        ,   @CodigoTipoObjeto
                        ,   70
                        ,   270
                        ,   84
                        ,   520
                        ,   '#FFFFFF'
                        ,   '#800000'
                        ,   '#000000'
                        ,   3
                        ,   {7}
                        ,   GETDATE()
                        ,   {8}
                        ,   {9}
                        ,   '{10}'
                        ,   '{11}'
                        ,   {12}
                        ,   {13})

                    SET @CodigoObjetivoEstrategico = SCOPE_IDENTITY()


                    --Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'OB'       
                    SET @in_codigoObjeto = @CodigoObjetivoEstrategico
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {14}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {7}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL




                    SELECT @CodigoObjetivoEstrategico AS Codigo, 'OK' AS Resultado
                END
            ", bancodb, Ownerdb, codigoMapaEstrategico, codigoVersaoMapa, iniciaisTipoObjeto
             , descricaoObjetoEstrategia
             , glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL"
             , idUsuarioInclusao
             , idResponsavelObjeto != -1 ? idResponsavelObjeto.ToString() : "NULL"
             , idObjetoSuperior, TituloObjetoEstrategia
             , classificacaoObjetivo.Replace("'", "''")
             , inicioValidade, terminoValidade, codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            codigoNovoObjetivoEstrategico = ds.Tables[0].Rows[0]["Codigo"].ToString();
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaObjetivosMapa(string descricaoObjeto, string tituloObjetoEstrategia, int idUsuarioEdicao, string glossario
                                        , int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa
                                        , int idResponsavel, string classificacaoObjetivo, string inicioValidade, string terminoValidade, string codigoObjetoSuperior, ref string resultado, int codigoEntidade)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia WHERE TituloObjetoEstrategia = '{9}' AND CodigoObjetoEstrategia != {5} AND CodigoMapaEstrategico = {6} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'OK' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     DescricaoObjetoEstrategia       = '{2}'
                        ,   GlossarioObjeto                 = {3}
                        ,   CodigoUsuarioUltimaAlteracao    = {4}
                        ,   DataUltimaAlteracao             = GETDATE()
                        ,   CodigoResponsavelObjeto         = {8}
                        ,   TituloObjetoEstrategia          = '{9}'
                        ,   ClassificacaoObjetoEstrategia   = '{10}'
                        ,   InicioValidade                  =  {11}
                        ,   TerminoValidade                 =  {12}
                        ,   CodigoObjetoEstrategiaSuperior  =  {13}
                    WHERE   CodigoObjetoEstrategia      = {5}
                        AND CodigoMapaEstrategico       = {6}
                        AND CodigoVersaoMapaEstrategico = {7}

                    --Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'OB'       
                    SET @in_codigoObjeto = {5}
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {14}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {7}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL

                    SELECT 'OK' AS Resultado
                END
                ", bancodb, Ownerdb, descricaoObjeto
                 , (glossario != "" ? "'" + glossario.Trim().Replace("'", "''") + "'" : "NULL")
                 , idUsuarioEdicao, idObjetoEstrategia
                 , idMapaEstrategia, idVersaoMapa, idResponsavel, tituloObjetoEstrategia
             , classificacaoObjetivo.Replace("'", "''")
             , inicioValidade, terminoValidade, codigoObjetoSuperior, codigoEntidade);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool excluiObjetivosMapa(int idObjetoEstrategia, int idMapaEstrategia, int idVersaoMapa, int idUsuarioExclusao
                                    , ref string resultado)
    {
        bool retorno = false;

        string comandoSQL = string.Format(@"
            IF EXISTS(SELECT 1 FROM {0}.{1}.ObjetoEstrategia where CodigoObjetoEstrategiaSuperior = {3} AND DataExclusao IS NULL)
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.ObjetoEstrategia

                    SET     CodigoUsuarioExclusao   = {2}
                        ,   DataExclusao            = GETDATE()

                    WHERE   CodigoObjetoEstrategia      = {3}
                        AND CodigoMapaEstrategico       = {4}
                        AND CodigoVersaoMapaEstrategico = {5}
                    
                    SELECT 'OK' AS Resultado

                END
                ", bancodb, Ownerdb, idUsuarioExclusao
                 , idObjetoEstrategia, idMapaEstrategia, idVersaoMapa);

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            resultado = ds.Tables[0].Rows[0]["Resultado"].ToString();
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region _Estrategias - Dashboard

    public DataSet getPerspectivasTemas(int codigoMapa, int mes, int ano, string where)
    {
        string comandoSQL = string.Format(
             @"/* 
  Objetivos: Consulta para apresentar as informações sobre perspectiva e tema, trazendo para cada tema a  
                                    cor que corresponde ao seu desempenho, levando-se em consideração cada um de seus indicadores
                                   (pior cenário).
                         Parâmetros: MapaEstratégico, Ano e Mês. A unidade de negócio deverá ser obtida de acordo com a entidade
                                     cadastrada no mapa estratégico.
*/

                        BEGIN 
                                DECLARE @CodigoMapa Int
                                DECLARE @CodigoEntidadeMapa Int
                                DECLARE @Ano Int
                                DECLARE @Mes Int                              

                                SET @CodigoMapa = {2}
                                SET @Ano = {3}
                                SET @Mes = {4}

                                SELECT @CodigoEntidadeMapa = CodigoUnidadeNegocio
                                  FROM {0}.{1}.MapaEstrategico
                                 WHERE CodigoMapaEstrategico = @CodigoMapa                         

                                SELECT p.TituloObjetoEstrategia AS Perspectiva,
                                       t.DescricaoObjetoEstrategia AS Tema,    
                                       {0}.{1}.f_GetCorDesempenhoResultado(Avg(ri.MetaAcumuladaAno),Avg(ri.ResultadoAcumuladoAno),'POS', 'IN', null) AS CorTema,
									   p.CodigoObjetoEstrategia,
                                       p.OrdemObjeto
                                  FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                                       {0}.{1}.RelacaoObjetoEstrategia AS roe ON (roe.CodigoObjetoEstrategiaPara = oe.CodigoObjetoEstrategia) INNER JOIN
                                       {0}.{1}.ObjetoEstrategia AS t ON (roe.CodigoObjetoEstrategiaDe = t.CodigoObjetoEstrategia)INNER JOIN 
                                       {0}.{1}.ObjetoEstrategia AS p ON (p.CodigoObjetoEstrategia = oe.CodigoObjetoEstrategiaSuperior) INNER JOIN
                                       {0}.{1}.IndicadorObjetivoEstrategico AS oi ON (oi.CodigoObjetivoEstrategico = oe.CodigoObjetoEstrategia) INNER JOIN
                                       {0}.{1}.ResumoIndicador AS ri ON (ri.CodigoIndicador = oi.CodigoIndicador 
                                                                                           AND ri.CodigoUnidadeNegocio = @CodigoEntidadeMapa) INNER JOIN
                                       {0}.{1}.Indicador AS i ON (i.CodigoIndicador = ri.CodigoIndicador
                                                                                     AND i.DataExclusao IS NULL)
                                 WHERE ri.Ano = @Ano
                                   AND ri.Mes = @Mes
                                   AND oe.CodigoMapaEstrategico = @CodigoMapa                                    
                                   {5}
                                GROUP BY p.TituloObjetoEstrategia,
                                         t.DescricaoObjetoEstrategia,
                                         p.CodigoObjetoEstrategia,
                                         p.OrdemObjeto
                                ORDER BY p.OrdemObjeto DESC 
                        END 
               ", bancodb, Ownerdb, codigoMapa, ano, mes, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Estrategias - vistaMapaEstrategico.aspx

    public DataSet getMapaEstrategicoToXML(string codigoMapa, string codigoUnidade)
    {
        string comandoSQL = string.Format(@"
                --Seleccionar El Mapa:
                SELECT  me.CodigoMapaEstrategico, me.TituloMapaEstrategico, me.CodigoUnidadeNegocio
                    ,   vme.VersaoMapaEstrategico, vme.DataInicioVersaoMapaEstrategico

                FROM        {0}.{1}.MapaEstrategico         AS me
                INNER JOIN  {0}.{1}.VersaoMapaEstrategico   AS vme ON me.CodigoMapaEstrategico = vme.CodigoMapaEstrategico

                WHERE   me.CodigoMapaEstrategico        = {2}
                    --AND me.IndicaMapaEstrategicoAtivo   = 'S'
                    --AND me.CodigoUnidadeNegocio         IN (SELECT CodigoUnidadeNegocio FROM UnidadeNegocio WHERE CodigoEntidade = {3}) --me.CodigoUnidadeNegocio = {3}
               ", bancodb, Ownerdb, codigoMapa, codigoUnidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getObjetosMapaEstrategicoToXML(string codigoMapaEstrategico, string codigoUsuario, string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                            SELECT ob.CodigoObjetoEstrategia, ob.CodigoVersaoMapaEstrategico, ob.TituloObjetoEstrategia, ob.DescricaoObjetoEstrategia
	                            ,  ob.CorFundoObjetoEstrategia, ob.CorBordaObjetoEstrategia, ob.CorFonteObjetoEstrategia
                                ,  ob.AlturaObjetoEstrategia, ob.LarguraObjetoEstrategia, ob.TopoObjetoEstrategia, ob.EsquerdaObjetoEstrategia
	                            ,  ob.CodigoObjetoEstrategiaSuperior, ob.CodigoTipoObjetoEstrategia, ob.OrdemObjeto, ob.NumRotacaoObjeto
	                            ,  ob.GlossarioObjeto, ob.DataInclusao, ob.CodigoUsuarioInclusao, ob.DataUltimaAlteracao, ob.CodigoUsuarioUltimaAlteracao
	                            ,  ob.CodigoResponsavelObjeto
	                            ,  {0}.{1}.f_GetEstruturaHierarquicaObjetivoEstrategico(ob.CodigoObjetoEstrategia) AS EstruturaHierarquica
                                ,  roe.CodigoObjetoEstrategiaDe, roe.CodigoObjetoEstrategiaPara
                                , ioe.IniciaisTipoObjeto
                                , {0}.{1}.f_VerificaAcessoConcedido({4}, {5}, ob.CodigoObjetoEstrategia, NULL, 'OB', 0, NULL, 'OB_CnsDtl') AS IndicaConsultaDetalhesPermitida
                            FROM {0}.{1}.ObjetoEstrategia ob
		                         LEFT JOIN {0}.{1}.RelacaoObjetoEstrategia roe
		                         ON ob.CodigoObjetoEstrategia = roe.CodigoObjetoDesenhoCausaEfeito
                                 LEFT JOIN {0}.{1}.TipoObjetoEstrategia ioe
                                 ON ob.CodigoTipoObjetoEstrategia = ioe.CodigoTipoObjetoEstrategia
                            WHERE ob.CodigoMapaEstrategico = {2}
			                      AND 
                                  ob.DataExclusao IS NULL
                                  {3}
                            ORDER BY EstruturaHierarquica
               ", bancodb, Ownerdb, codigoMapaEstrategico, where, codigoUsuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getParametroSistemaToXML(string codigoUnidade)
    {
        string comandoSQL = string.Format(@"
			                SELECT
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaMissao' AND CodigoEntidade = {2}) as 'corBordaMissao',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoMissao' AND CodigoEntidade = {2}) as 'corFundoMissao',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteMissao' AND CodigoEntidade = {2}) as 'corFonteMissao',

                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaVisao' AND CodigoEntidade = {2}) as 'corBordaVisao',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoVisao' AND CodigoEntidade = {2}) as 'corFundoVisao',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteVisao' AND CodigoEntidade = {2}) as 'corFonteVisao',

                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaPerspectiva1' AND CodigoEntidade = {2}) as 'corBordaPerspectiva1',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoPerspectiva1' AND CodigoEntidade = {2}) as 'corFundoPerspectiva1',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFontePerspectiva1' AND CodigoEntidade = {2}) as 'corFontePerspectiva1',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaObjetivosPerspectiva1' AND CodigoEntidade = {2}) as 'corBordaObjetivosPerspectiva1',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoObjetivosPerspectiva1' AND CodigoEntidade = {2}) as 'corFundoObjetivosPerspectiva1',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteObjetivosPerspectiva1' AND CodigoEntidade = {2}) as 'corFonteObjetivosPerspectiva1',

                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaPerspectiva2' AND CodigoEntidade = {2}) as 'corBordaPerspectiva2',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoPerspectiva2' AND CodigoEntidade = {2}) as 'corFundoPerspectiva2',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFontePerspectiva2' AND CodigoEntidade = {2}) as 'corFontePerspectiva2',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaObjetivosPerspectiva2' AND CodigoEntidade = {2}) as 'corBordaObjetivosPerspectiva2',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoObjetivosPerspectiva2' AND CodigoEntidade = {2}) as 'corFundoObjetivosPerspectiva2',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteObjetivosPerspectiva2' AND CodigoEntidade = {2}) as 'corFonteObjetivosPerspectiva2',

                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaPerspectiva3' AND CodigoEntidade = {2}) as 'corBordaPerspectiva3',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoPerspectiva3' AND CodigoEntidade = {2}) as 'corFundoPerspectiva3',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFontePerspectiva3' AND CodigoEntidade = {2}) as 'corFontePerspectiva3',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaObjetivosPerspectiva3' AND CodigoEntidade = {2}) as 'corBordaObjetivosPerspectiva3',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoObjetivosPerspectiva3' AND CodigoEntidade = {2}) as 'corFundoObjetivosPerspectiva3',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteObjetivosPerspectiva3' AND CodigoEntidade = {2}) as 'corFonteObjetivosPerspectiva3',

                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaPerspectiva4' AND CodigoEntidade = {2}) as 'corBordaPerspectiva4',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoPerspectiva4' AND CodigoEntidade = {2}) as 'corFundoPerspectiva4',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFontePerspectiva4' AND CodigoEntidade = {2}) as 'corFontePerspectiva4',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corBordaObjetivosPerspectiva4' AND CodigoEntidade = {2}) as 'corBordaObjetivosPerspectiva4',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFundoObjetivosPerspectiva4' AND CodigoEntidade = {2}) as 'corFundoObjetivosPerspectiva4',
                (SELECT Valor FROM {0}.{1}.ParametroConfiguracaoSistema WHERE Parametro = 'corFonteObjetivosPerspectiva4' AND CodigoEntidade = {2}) as 'corFonteObjetivosPerspectiva4'
               ", bancodb, Ownerdb, codigoUnidade);
        return getDataSet(comandoSQL);
    }

    public string getCodigoTipoObjeto(string iniciaisTipoObjeto)
    {
        DataTable dt;
        string codigoTipoObjeto = "";

        string comandoSQL = string.Format(@"SELECT CodigoTipoObjetoEstrategia FROM {0}.{1}.TipoObjetoEstrategia WHERE IniciaisTipoObjeto = '{2}'", bancodb, Ownerdb, iniciaisTipoObjeto);
        dt = getDataSet(comandoSQL).Tables[0];
        if (DataTableOk(dt))
            codigoTipoObjeto = dt.Rows[0]["CodigoTipoObjetoEstrategia"].ToString();

        return codigoTipoObjeto;
    }

    public string getTipoObjetoPorCodigo(int codigoObjeto)
    {
        DataTable dt;
        string codigoTipoObjeto = "";

        string comandoSQL = string.Format(@"SELECT CodigoTipoProjeto 
                                              FROM {0}.{1}.Projeto 
                                             WHERE CodigoProjeto = {2}", bancodb, Ownerdb, codigoObjeto);
        dt = getDataSet(comandoSQL).Tables[0];
        if (DataTableOk(dt))
            codigoTipoObjeto = dt.Rows[0]["CodigoTipoProjeto"].ToString();

        return codigoTipoObjeto;
    }

    #endregion

    #region _Estrategias - InteressadoObjetoEstrategia.aspx

    public DataSet getInteressadosObjetos(string iniciaisTA, int idObjetoAssociado, int idObjetoPai, int idEntidade)
    {
        comandoSQL = string.Format(@"
                    SELECT  iobj.[CodigoInteressado]        AS [CodigoUsuario]
                        ,   iobj.[NomeInteressado]          AS [NomeUsuario]
                        ,   ISNULL(iobj.[Perfis], '*')      AS [Perfis]
                        ,   CAST({3} AS BigInt)             AS [CodigoObjetoAssociado]
                        ,   ta.[CodigoTipoAssociacao]
                        ,   ta.[IniciaisTipoAssociacao]
                        ,   iobj.[HerdaPermissoesObjetoSuperior]
                        ,   iobj.[IndicaPermissoesPersonalizadas]
                        ,   iobj.[IndicaEdicaoPermitida]


                    FROM    {0}.{1}.f_GetInteressadosObjeto( {3}, NULL, '{2}', {4}, {5}, 'T')   AS [iobj]
                        ,   {0}.{1}.TipoAssociacao                                              AS [ta]

                    WHERE   ta.[IniciaisTipoAssociacao]	= '{2}'

                    ORDER BY iobj.[NomeInteressado]
                ", bancodb, Ownerdb, iniciaisTA, idObjetoAssociado, idObjetoPai, idEntidade);

        return getDataSet(comandoSQL);
    }

    public bool getAcessoRestringidoDaPermissao(int idObjetoAssociado, int idEntidadeLogada, int idObjetoPai, string iniciaisTipoAsociacao)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
                    DECLARE @CodigoTipoAssociacao Int
                    SELECT  @CodigoTipoAssociacao = CodigoTipoAssociacao FROM  TipoAssociacao WHERE IniciaisTipoAssociacao = '{4}'

					SELECT IndicaAcessoRestrito
					FROM {0}.{1}.TipoAcessoObjeto
					WHERE CodigoEntidade = {2}
						AND CodigoObjetoAssociado = {3}
						AND CodigoTipoAssociacao = @CodigoTipoAssociacao
						AND CodigoObjetoPai = {5}
                    ", getDbName(), getDbOwner(), idEntidadeLogada, idObjetoAssociado, iniciaisTipoAsociacao, idObjetoPai);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            retorno = ds.Tables[0].Rows[0]["IndicaAcessoRestrito"].ToString().Equals("S");

        return retorno;
    }

    public void atualizaAcessoRestringidoDaPermissao(int idObjetoAssociado, int idEntidadeLogada, int idObjetoPai, int idUsuarioAlteracao
                                                    , string iniciaisTipoAsociacao, string indicaAcessoRestringido)
    {
        string comandoSQL = string.Format(@"
                    DECLARE @CodigoTipoAssociacao Int
                    SELECT  @CodigoTipoAssociacao = CodigoTipoAssociacao FROM  TipoAssociacao WHERE IniciaisTipoAssociacao = '{4}'

					IF NOT EXISTS(SELECT 1
					            FROM {0}.{1}.TipoAcessoObjeto
					            WHERE CodigoEntidade        = {2}
						          AND CodigoObjetoAssociado = {3}
						          AND CodigoTipoAssociacao  = @CodigoTipoAssociacao
						          AND CodigoObjetoPai       = {5}
                                )
                    BEGIN
                        INSERT INTO {0}.{1}.TipoAcessoObjeto (CodigoEntidade, CodigoObjetoAssociado, CodigoTipoAssociacao
                                                                , CodigoObjetoPai, IndicaAcessoRestrito, DataUltimaAtualizacao
                                                                , CodigoUsuarioUltimaAtualizacao)
                        VALUES({2}, {3}, @CodigoTipoAssociacao, {5}, '{6}', GETDATE(), {7})
                    END
                    ELSE
                    BEGIN
                        UPDATE {0}.{1}.TipoAcessoObjeto
                        SET IndicaAcessoRestrito            = '{6}'
                        , DataUltimaAtualizacao             = GETDATE()
                        , CodigoUsuarioUltimaAtualizacao    = {7}

                        WHERE CodigoEntidade        = {2}
                        AND CodigoObjetoAssociado   = {3}
                        AND CodigoTipoAssociacao    = @CodigoTipoAssociacao
                        AND CodigoObjetoPai         = {5}
                    END
                    ", getDbName(), getDbOwner(), idEntidadeLogada, idObjetoAssociado, iniciaisTipoAsociacao, idObjetoPai
                     , indicaAcessoRestringido.Equals("true") ? "S" : "N"
                     , idUsuarioAlteracao);

        DataSet ds = getDataSet(comandoSQL);

    }

    public DataSet getListaPerfisDisponiveisUsuario(int codigoObjeto, string tipoAssociacao, int codigoObjetoPai
                                                  , int idEntidade, int codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                DECLARE @retorno AS Int,
                        @codigoTipoAssociacao   AS int

                SELECT  @codigoTipoAssociacao = [CodigoTipoAssociacao] 
                FROM    {0}.{1}.TipoAssociacao 
                WHERE   IniciaisTipoAssociacao = '{2}'

                EXEC @retorno = {0}.{1}.p_perm_obtemPerfisDisponiveisUsuario
                                        @in_codigoObjeto            = {3}, 
                                        @in_codigoTipoAssociacao    = @codigoTipoAssociacao,
                                        @in_iniciaisTipoAssociacao  = '{2}',
                                        @in_codigoObjetoPai         = {4},
                                        @in_codigoEntidade          = {5},
                                        @in_codigoUsuario           = {6}
                ", bancodb, Ownerdb, tipoAssociacao
                 , codigoObjeto
                 , codigoObjetoPai
                 , idEntidade
                 , codigoUsuario);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaPerfisAtribuidosUsuario(int codigoObjeto, string tipoAssociacao
                                                    , int codigoObjetoPai, int idEntidade, int codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                DECLARE @retorno AS Int
                    --,   @codigoTipoAssociacao   AS int

                --SELECT  @codigoTipoAssociacao = [CodigoTipoAssociacao] 
                --FROM    {0}.{1}.TipoAssociacao 
                --WHERE   IniciaisTipoAssociacao = '{2}'

                EXEC @retorno = {0}.{1}.p_perm_obtemPerfisAtribuidosUsuario
                                @in_codigoObjeto            = {3},
                                @in_codigoTipoAssociacao    = NULL, -- @codigoTipoAssociacao,
                                @in_iniciaisTipoAssociacao  = '{2}',
                                @in_codigoObjetoPai         = {4},
                                @in_codigoEntidade          = {5},
                                @in_codigoUsuario           = {6}
                ", bancodb, Ownerdb, tipoAssociacao
                 , codigoObjeto
                 , codigoObjetoPai
                 , idEntidade
                 , codigoUsuario);
        return getDataSet(comandoSQL);
    }

    //função Ok.
    public bool incluirInteressadoObjeto(int codigoObjetoAssociacao, string codigoTipoAssociacao, string iniciaisTipoAssociacao
                                         , int codigoUsuario, bool herdaPermissaoObjetoSuperior
                                         , int idUsuarioLogado, int codigoObjetoPai
                                         , string listaPermissao, int idEntidade, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @retorno AS Int, 
                        @codigoTipoAssociacao   AS int

                --SELECT  @codigoTipoAssociacao = [CodigoTipoAssociacao] 
                --FROM    {0}.{1}.TipoAssociacao 
                --WHERE   IniciaisTipoAssociacao = '{2}'

                EXEC @retorno = {0}.{1}.p_perm_incluiInteressadoObjetoPerfil
                                @in_codigoObjeto                    = {3}, 
                                @in_codigoTipoAssociacao            = {10},
                                @in_iniciaisTipoAssociacao          = {2},
                                @in_codigoObjetoPai                 = {7},
                                @in_codigoEntidade                  = {9},
                                @in_codigoUsuarioInteressado        = {4},
                                @in_herdaPermissoesObjetoSuperior   = {5},
                                @in_codigoUsuarioInclusao           = {6},
                                @in_perfisSelecionados              = '{8}'
            END
            ", bancodb, Ownerdb, iniciaisTipoAssociacao
             , codigoObjetoAssociacao, codigoUsuario
             , herdaPermissaoObjetoSuperior ? "'S'" : "'N'"
             , idUsuarioLogado, codigoObjetoPai, listaPermissao //, aplicaPermissoes, listaPermissao
             , idEntidade, codigoTipoAssociacao);

            int regAfetados = 0;
            execSQL(comandoSQL, ref regAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    //função Ok.
    public bool registraPermissoesUsuario(string PermissaoDoInteressado, string tipoAssociacao
                                                   , int codigoObjetoAssociado, int codigoUsuario
                                                   , int codigoUsuarioInclusao, int codigoObjetoPai
                                                   , int idEntidade
                                                   , ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                DECLARE @retorno AS Int, 
                        @CodigoTipoAssociacao   AS int,
                        @CodigoObjetoAssociado  AS int,
                        @CodigoUsuarioPK        AS int

                SET @CodigoObjetoAssociado  = {3}

                --SELECT  @codigoTipoAssociacao = [CodigoTipoAssociacao] 
                --FROM    {0}.{1}.TipoAssociacao 
                --WHERE   IniciaisTipoAssociacao = '{2}'

                SET @CodigoUsuarioPK  = {5}

                EXEC @retorno = {0}.{1}.p_perm_registraPermissoesUsuario
                                  @in_codigoObjeto              = @CodigoObjetoAssociado
                                , @in_codigoTipoAssociacao      = NULL --@codigoTipoAssociacao
                                , @in_iniciaisTipoAssociacao    = '{2}'
                                , @in_codigoObjetoPai           = {6}
                                , @in_codigoEntidade            = {8}
                                , @in_codigoUsuarioInteressado  = @CodigoUsuarioPK
                                , @in_permissoes                = '{4}'
                                , @in_codigoUsuarioInclusao     = {7}

            ", bancodb, Ownerdb, tipoAssociacao
             , codigoObjetoAssociado, PermissaoDoInteressado
             , codigoUsuario, codigoObjetoPai, codigoUsuarioInclusao, idEntidade);

            int regAfetados = 0;
            execSQL(comandoSQL, ref regAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiPermissaoObjetoAoUsuario(int codigoTipoAssociacao, int codigoObjetoAssociado
                                                , int codigoUsuario, int idUsuarioLogado, string iniciaisObjeto
                                                , int codigoObjetoPai, int idEntidadeLogada, bool herdaPermissao
                                                , ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
                DECLARE @retorno AS Int

                --1) fazer um update, desativando as permissao do interessado (StatusRegistro, DataDesativacaoRegistro, CodigoUsuarioDesativacao).
                UPDATE  {0}.{1}.InteressadoObjetoPermissao
                SET     StatusRegistro = 'D'
                    ,   DataDesativacaoRegistro = GETDATE()
                    ,   CodigoUsuarioDesativacao = {5}

                WHERE   CodigoTipoAssociacao    = {2} 
	              AND   CodigoObjetoAssociado   = {3} 
	              AND   CodigoUsuario           = {4}

                --2) Desativar os perfil do interessado ao objeto:
                EXEC @retorno = {0}.{1}.p_perm_incluiInteressadoObjetoPerfil
                                @in_codigoObjeto                    = {3}, 
                                @in_codigoTipoAssociacao            = NULL,
                                @in_iniciaisTipoAssociacao          = '{6}',
                                @in_codigoObjetoPai                 = {7},
                                @in_codigoEntidade                  = {8},
                                @in_codigoUsuarioInteressado        = {4},
                                @in_herdaPermissoesObjetoSuperior   = {9},
                                @in_codigoUsuarioInclusao           = {5},
                                @in_perfisSelecionados              = ';'

                --3) fazer um update... na dataDesativacaom, codigousuariodes.., estatusRe...
                UPDATE  {0}.{1}.InteressadoObjeto
                SET     StatusRegistro = 'D'
                    ,   DataDesativacaoRegistro = GETDATE()
                    ,   CodigoUsuarioDesativacao = {5}

                WHERE   CodigoTipoAssociacao    = {2} 
	              AND   CodigoObjetoAssociado   = {3} 
	              AND   CodigoUsuario           = {4}


        ", bancodb, Ownerdb, codigoTipoAssociacao, codigoObjetoAssociado, codigoUsuario, idUsuarioLogado
         , iniciaisObjeto, codigoObjetoPai, idEntidadeLogada
         , herdaPermissao ? "'S'" : "'N'");
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    #endregion

    #region Mapa Estratégico

    public DataSet getTipoObjetoEstrategia(string IniciaisTipoObjeto)
    {
        comandoSQL = string.Format(
            @"SELECT * 
                FROM {0}.{1}.TipoObjetoEstrategia 
               WHERE IniciaisTipoObjeto = '{2}'", bancodb, Ownerdb, IniciaisTipoObjeto);
        return getDataSet(comandoSQL);
    }

    public string getSelect_MapaEstrategico(int codigoEntidadeLogada, int idUsuario, string where)
    {
        return string.Format(@"
                SELECT  Mapa.CodigoMapaEstrategico
                    ,   Mapa.TituloMapaEstrategico
                    ,   Mapa.IndicaMapaEstrategicoAtivo
                    ,   Ver.DataInicioVersaoMapaEstrategico
                    ,   Ver.VersaoMapaEstrategico
                    ,   Ver.DataTerminoVersaoMapaEstrategico
                    ,   Mapa.CodigoUnidadeNegocio

                    ,   {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_Alt')      *   2   +	-- 2	alterar
                      --{0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_AdmPrs')   *   4	+	-- 4	excluir
                        {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_AdmPrs')   *   8	+	-- 8	administrar permissões
                        {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_Compart')  *   16      -- 16	Compartilhar
                        AS [Permissoes]

                FROM        {0}.{1}.MapaEstrategico         AS Mapa
                INNER JOIN  {0}.{1}.VersaoMapaEstrategico   AS Ver      ON Mapa.CodigoMapaEstrategico   = Ver.CodigoMapaEstrategico 
                INNER JOIN  {0}.{1}.UnidadeNegocio          AS Un       ON Un.CodigoUnidadeNegocio      = Mapa.CodigoUnidadeNegocio

                WHERE   
                    Mapa.CodigoMapaEstrategico IN (SELECT CodigoMapaEstrategico FROM {0}.{1}.f_GetMapasEstrategicosUsuario({4},{3})) AND  
                (    {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_Alt')    = 1 
                  OR {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_AdmPrs') = 1
                  OR {0}.{1}.f_VerificaAcessoConcedido({4}, {3}, Mapa.CodigoMapaEstrategico, NULL, 'ME', 0, NULL, 'ME_Compart')= 1
                )  
                {2}
            ", bancodb, Ownerdb, where, codigoEntidadeLogada, idUsuario);
    }

    public void getExisteNomeMapaEstrategicoNoBanco(int codigoUnidade, string nomeMapa, int codigoMapa, int idUsuario, ref bool existeNomeBAnco)
    {
        existeNomeBAnco = false;
        string commandoSql = getSelect_MapaEstrategico(codigoUnidade, idUsuario, " AND Mapa.TituloMapaEstrategico = '" + nomeMapa.Trim() + "'");
        DataSet ds = getDataSet(commandoSql);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            int codigoMapaBanco = int.Parse(ds.Tables[0].Rows[0]["CodigoMapaEstrategico"].ToString());
            if (codigoMapa != codigoMapaBanco)
                existeNomeBAnco = true;
        }
    }

    public DataSet getMapasEstrategicos(int? codigoEntidade, string where)
    {
        if (codigoEntidade.HasValue)
            where = " AND un.CodigoEntidade = " + codigoEntidade.Value + " " + where;

        comandoSQL = string.Format(@"
                SELECT  Mapa.CodigoMapaEstrategico, Mapa.TituloMapaEstrategico, Mapa.IndicaMapaEstrategicoAtivo
                    ,   Ver.DataInicioVersaoMapaEstrategico, Ver.VersaoMapaEstrategico, Ver.DataTerminoVersaoMapaEstrategico
                    ,   Mapa.CodigoUnidadeNegocio
                    ,   ISNULL(un.CodigoEntidade, Un.CodigoUnidadeNegocio) AS CodigoEntidade
					,   ISNULL(Mapa.IndicaMapaCarregado, 'N') AS IndicaMapaCarregado
                FROM        {0}.{1}.MapaEstrategico         AS Mapa 
                INNER JOIN  {0}.{1}.VersaoMapaEstrategico   AS Ver      ON Mapa.CodigoMapaEstrategico   = Ver.CodigoMapaEstrategico 
                INNER JOIN  {0}.{1}.UnidadeNegocio          AS Un       ON Un.CodigoUnidadeNegocio      = Mapa.CodigoUnidadeNegocio

                WHERE 1 = 1 
                {2}
                 ORDER BY Mapa.TituloMapaEstrategico
                ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getMapasUsuarioEntidade(string codigoEntidade, int codigoUsuario, string where)
    {
        int codigoEntidadeLogado = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        comandoSQL = string.Format(@"
                    SELECT  me.CodigoMapaEstrategico, me.TituloMapaEstrategico
                        ,   CASE    WHEN    un.CodigoEntidade = {5} 
                                    THEN    '~/imagens/MapaEntidade.png' 
                                    ELSE    '~/imagens/MapaOutraEntidade.png' 
                                    END     AS imagemMapaUnidade

                    FROM        {0}.{1}.MapaEstrategico                         AS me
                    INNER JOIN  {0}.{1}.f_GetMapasEstrategicosUsuario({2}, {3}) AS f    ON f.CodigoMapaEstrategico = me.CodigoMapaEstrategico
                    INNER JOIN  {0}.{1}.UnidadeNegocio                          AS un   ON un.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio

                    WHERE me.IndicaMapaEstrategicoAtivo = 'S'
                      {4}
                    ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, where, codigoEntidadeLogado);

        return getDataSet(comandoSQL);
    }

    public DataSet getListaProjetosMapa(int codigoMapa, int codigoUsuario, string where)
    {
        comandoSQL = string.Format(@"
                    BEGIN 
                      DECLARE @CodigoMapaEstrategico INT,
                              @CodigoUsuario INT
  
                      /* Parâmetro a ser passado ao script */
                      SET @CodigoMapaEstrategico = {3}
                      SET @CodigoUsuario = {2}
  
                      DECLARE @tblRetorno TABLE
                        (CodigoProjeto INT,
                         NomeProjeto Varchar(500),
                         FatorChave Varchar(500),
                         Tema Varchar(500),
                         ObjetivoEstrategico Varchar(500),
                         AcaoTransformadora Varchar(500),
                         Unidade Varchar(200),
                         GerenteUnidade Varchar(150),
                         GerenteProjeto Varchar(150),
                         PercentualPrevisto Int,
                         PercentualConcluido Int,
                         CorDesempenho Varchar(30),
                         PodeAcessarProjeto Char(1),
                         IndicaProjetoCarteiraPrioritaria Char(1),
                         StatusProjeto VarChar(50))     
     
                      DECLARE @CodigoFatorChave INT,
                              @FatorChave Varchar(500)
  
                      DECLARE cCursor CURSOR LOCAL FOR
                      SELECT oe.CodigoObjetoEstrategia,
                             oe.TituloObjetoEstrategia
                        FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                             {0}.{1}.TipoObjetoEstrategia AS toe ON (oe.CodigoTipoObjetoEstrategia = toe.CodigoTipoObjetoEstrategia
                                                         AND toe.IniciaisTipoObjeto = 'PSP')
                       WHERE oe.DataExclusao IS NULL
                         AND oe.CodigoMapaEstrategico = @CodigoMapaEstrategico                                     
     
                      OPEN cCursor
  
                      FETCH NEXT FROM cCursor INTO @CodigoFatorChave, @FatorChave
  
                      WHILE @@FETCH_STATUS = 0
                        BEGIN
                          INSERT INTO @tblRetorno
			                     (CodigoProjeto,
				                    NomeProjeto,
				                    FatorChave,
				                    Tema,
				                    ObjetivoEstrategico,
				                    AcaoTransformadora,
				                    Unidade,
				                    GerenteUnidade,
                                    GerenteProjeto,
				                    PercentualPrevisto,
				                    PercentualConcluido,
				                    CorDesempenho,
				                    PodeAcessarProjeto,
                                    IndicaProjetoCarteiraPrioritaria,
                                    StatusProjeto)
			                    SELECT f.CodigoProjeto,
			                           f.NomeProjeto,
			                           @FatorChave,
			                           tema.TituloObjetoEstrategia,
			                           f.NomeObjetivoEstrategico,
			                           f.AcaoTransformadora,
			                           un.NomeUnidadeNegocio,
			                           u.NomeUsuario,
			                           upr.NomeUsuario,
			                           f.PercentualPrevisto,
			                           f.PercentualConcluido,
			                           RTRIM(LTRIM(f.CorDesempenho)),
			                           f.PodeAcessarProjeto,
                                       f.IndicaProjetoCarteiraPrioritaria,
                                       s.DescricaoStatus
			                      FROM {0}.{1}.[f_cni_getProjetosObjetoPorAno](@CodigoFatorChave, 'PP', @CodigoUsuario, YEAR(GETDATE())) AS f INNER JOIN
			                           {0}.{1}.ObjetoEstrategia AS oe ON (oe.CodigoObjetoEstrategia = f.CodigoObjetivoEstrategico) INNER JOIN
			                           {0}.{1}.ObjetoEstrategia AS tema ON (tema.CodigoObjetoEstrategia = oe.CodigoObjetoEstrategiaSuperior) INNER JOIN
			                           {0}.{1}.Projeto AS p ON (p.CodigoProjeto = f.CodigoProjeto) INNER JOIN
			                           {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN
			                           {0}.{1}.Usuario AS u ON (u.CodigoUsuario = un.CodigoUsuarioGerente) LEFT JOIN
			                           {0}.{1}.Usuario AS upr ON (upr.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN 
                                       {0}.{1}.Status s ON s.CodigoStatus = p.CodigoStatusProjeto
                           WHERE oe.DataExclusao IS NULL
                             AND tema.DataExclusao IS NULL
         			       
                          FETCH NEXT FROM cCursor INTO @CodigoFatorChave, @FatorChave
                        END
    
                      CLOSE cCursor
                      DEALLOCATE cCursor 
  
                      SELECT * 
                        FROM @tblRetorno  
                       WHERE 1 = 1
                         {4} 
                    END
                      
                    ", bancodb, Ownerdb, codigoUsuario, codigoMapa, where);

        return getDataSet(comandoSQL);
    }

    //criado no 11/01/2011 - verificar TESTAR
    public DataSet getDadosUsuarioEntidade(string codigoEntidade, int codigoUsuario, string where)
    {
        int codigoEntidadeLogado = int.Parse(getInfoSistema("CodigoEntidade").ToString());

        comandoSQL = string.Format(@"
                    SELECT  DISTINCT Dado, CodigoDado
                    FROM    {0}.{1}.[f_GetDadosOLAP_AnaliseDadosEstrategia] ({2}, {3}, NULL, NULL)
                    ORDER BY Dado ASC
                    ", bancodb, Ownerdb, codigoUsuario, codigoEntidadeLogado);

        return getDataSet(comandoSQL);
    }

    public void getAcessoPadraoMapaEstrategico(int codigoUsuario, int codigoEntidade, int codigoMapa, ref int codigoPadraoBanco)
    {
        codigoPadraoBanco = -1;

        comandoSQL = string.Format(@"
            SELECT f.CodigoMapaEstrategico
            FROM {0}.{1}.f_GetMapasEstrategicosUsuario({2},{3}) AS f
            WHERE f.CodigoMapaEstrategico = {4}
            ", bancodb, Ownerdb, codigoUsuario, codigoEntidade, codigoMapa);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            codigoPadraoBanco = int.Parse(ds.Tables[0].Rows[0]["CodigoMapaEstrategico"].ToString());
        }
    }

    public DataSet getDadosRelatorioMapa(int codigoMapa)
    {
        string comandoSQL = string.Format(@"
                                 SELECT      persp.[TituloObjetoEstrategia]      AS [Perspectiva]
                        ,   obj.[DescricaoObjetoEstrategia]     AS [Objetivo]
                        ,   CASE
                            WHEN la.DescricaoLinhaAtuacao LIKE '%_%' THEN  CAST( 'Linhas de Atuação' AS Varchar(30)) ELSE NULL END   AS [TipoDetalhe] 
                        ,   la.DescricaoLinhaAtuacao            AS [Detalhe]

                FROM {0}.{1}.[ObjetoEstrategia]                     AS [obj]
                    INNER JOIN {0}.{1}.[TipoObjetoEstrategia]       AS [tobj]
                            ON (    tobj.[CodigoTipoObjetoEstrategia]   = obj.[CodigoTipoObjetoEstrategia] 
                                AND tobj.[IniciaisTipoObjeto]           = 'OBJ')
                    INNER JOIN {0}.{1}.[ObjetoEstrategia]           AS [persp]     
                            ON ( persp.[CodigoObjetoEstrategia]         = obj.[CodigoObjetoEstrategiaSuperior])
                    INNER JOIN {0}.{1}.[TipoObjetoEstrategia]       AS [tpersp]    
                            ON (    tpersp.[CodigoTipoObjetoEstrategia] = persp.[CodigoTipoObjetoEstrategia] 
                                AND tpersp.[IniciaisTipoObjeto]         = 'PSP')
                    LEFT JOIN {0}.{1}.[LinhaAtuacaoEstrategica]     AS [la]
                            ON ( la.CodigoObjetoAssociado               = obj.[CodigoObjetoEstrategia]
                            AND la.DataDesativacao IS NULL)
                
                WHERE obj.[CodigoMapaEstrategico]   = {2}
                  AND obj.DataExclusao IS NULL
              
              UNION ALL
              
                 SELECT persp.[TituloObjetoEstrategia]      AS [Perspectiva]
                        ,obj.[DescricaoObjetoEstrategia]     AS [Objetivo]
                        ,CASE
                        WHEN la.DescricaoLinhaAtuacao LIKE '%_%' 
                        THEN CAST( 'Linhas de Atuação' AS Varchar(30)) ELSE NULL END AS [TipoDetalhe] 
                        ,la.DescricaoLinhaAtuacao AS [Detalhe]
                   FROM {0}.{1}.[ObjetoEstrategia]   AS [obj]
             INNER JOIN {0}.{1}.[TipoObjetoEstrategia]       AS [tobj] ON (tobj.[CodigoTipoObjetoEstrategia]   = obj.[CodigoTipoObjetoEstrategia] AND tobj.[IniciaisTipoObjeto] = 'OBJ')
             INNER JOIN {0}.{1}.ObjetoEstrategia AS tem ON (tem.CodigoObjetoEstrategia = obj.CodigoObjetoEstrategiaSuperior)     
             INNER JOIN {0}.{1}.TipoObjetoEstrategia AS toeTem ON (toeTem.CodigoTipoObjetoEstrategia = tem.CodigoTipoObjetoEstrategia AND toeTem.IniciaisTipoObjeto = 'TEM')       
             INNER JOIN {0}.{1}.[ObjetoEstrategia]           AS [persp] ON (persp.[CodigoObjetoEstrategia] = tem.[CodigoObjetoEstrategiaSuperior])
             INNER JOIN {0}.{1}.[TipoObjetoEstrategia]       AS [tpersp] ON (tpersp.[CodigoTipoObjetoEstrategia] = persp.[CodigoTipoObjetoEstrategia] AND tpersp.[IniciaisTipoObjeto]         = 'PSP')
              LEFT JOIN {0}.{1}.[LinhaAtuacaoEstrategica]     AS [la]
                            ON ( la.CodigoObjetoAssociado = obj.[CodigoObjetoEstrategia] AND la.DataDesativacao            IS NULL)
                  WHERE obj.[CodigoMapaEstrategico]   = {2} 
                        AND obj.DataExclusao IS NULL
                
                UNION ALL
                
                SELECT  persp.[TituloObjetoEstrategia]      AS [Perspectiva]
                        ,obj.[DescricaoObjetoEstrategia]     AS [Objetivo]
                        ,CASE  
                         WHEN la.[DescricaoAcao] like '%_%' 
                         THEN CAST( 'Ações Sugeridas' AS Varchar(30)) ELSE NULL END AS [TipoDetalhe] 
                        ,la.[DescricaoAcao]                  AS [Detalhe]
                   FROM {0}.{1}.[ObjetoEstrategia]                 AS [obj]
             INNER JOIN {0}.{1}.[TipoObjetoEstrategia]   AS [tobj] ON (tobj.[CodigoTipoObjetoEstrategia] = obj.[CodigoTipoObjetoEstrategia] AND tobj.[IniciaisTipoObjeto] = 'OBJ')
             INNER JOIN {0}.{1}.[ObjetoEstrategia]       AS [persp] ON (persp.[CodigoObjetoEstrategia] = obj.[CodigoObjetoEstrategiaSuperior])
             INNER JOIN {0}.{1}.[TipoObjetoEstrategia]   AS [tpersp] ON (tpersp.[CodigoTipoObjetoEstrategia] = persp.[CodigoTipoObjetoEstrategia] AND tpersp.[IniciaisTipoObjeto]  = 'PSP')
              LEFT JOIN {0}.{1}.[AcoesSugeridas]          AS [la] ON (la.CodigoObjetoAssociado  = obj.[CodigoObjetoEstrategia] AND la.DataDesativacao            IS NULL)
                  WHERE obj.[CodigoMapaEstrategico] = {2}     
                    AND obj.DataExclusao IS NULL
                
              UNION ALL
              
                SELECT persp.[TituloObjetoEstrategia]      AS [Perspectiva]
                       ,obj.[DescricaoObjetoEstrategia]     AS [Objetivo]
                       ,case  
                       WHEN la.[DescricaoAcao] like '%_%' 
                       THEN  CAST( 'Ações Sugeridas' AS Varchar(30)) ELSE NULL END AS [TipoDetalhe] 
                       ,la.[DescricaoAcao]                  AS [Detalhe]
                  FROM {0}.{1}.[ObjetoEstrategia]                 AS [obj]
            INNER JOIN {0}.{1}.[TipoObjetoEstrategia]   AS [tobj] ON (tobj.[CodigoTipoObjetoEstrategia]   = obj.[CodigoTipoObjetoEstrategia] AND tobj.[IniciaisTipoObjeto]           = 'OBJ')
            INNER JOIN {0}.{1}.[ObjetoEstrategia] AS tem ON (tem.CodigoObjetoEstrategia = obj.CodigoObjetoEstrategiaSuperior)
            INNER JOIN {0}.{1}.[TipoObjetoEstrategia] AS toeTem ON (toeTem.CodigoTipoObjetoEstrategia = tem.CodigoTipoObjetoEstrategia AND toeTem.IniciaisTipoObjeto = 'TEM')
            INNER JOIN {0}.{1}.[ObjetoEstrategia]       AS [persp] ON (persp.[CodigoObjetoEstrategia]          = tem.[CodigoObjetoEstrategiaSuperior])
            INNER JOIN {0}.{1}.[TipoObjetoEstrategia]   AS [tpersp] ON (    tpersp.[CodigoTipoObjetoEstrategia] = persp.[CodigoTipoObjetoEstrategia] AND tpersp.[IniciaisTipoObjeto]         = 'PSP')
             LEFT JOIN {0}.{1}.[AcoesSugeridas]          AS [la]  ON (la.CodigoObjetoAssociado                = obj.[CodigoObjetoEstrategia] AND la.DataDesativacao            IS NULL)
                   
                WHERE obj.[CodigoMapaEstrategico]   = {2}                   
                  AND obj.DataExclusao IS NULL
                
                ORDER BY 1, 2, 3, 4 ASC
", getDbName(), getDbOwner(), codigoMapa);
        return getDataSet(comandoSQL);
    }

    public DataSet getTreeMapaEstrategico(string codigoMapaEstrategico, string codigoEntidade, int codigoUsuario, string where)
    {
        comandoSQL = string.Format(@"
        
        BEGIN 

	        DECLARE @tbResumo TABLE(Codigo VarChar(20),
							        CodigoPai VarChar(20),
							        Descricao VarChar(500),
							        Cor VarChar(10),
							        TipoObjeto Char(3),
									Permissao bit)

            DECLARE @CodigoMapaEstrategico              AS INT
            DECLARE @CodigoEntidadLogada                AS INT
			DECLARE @CodigoUsuario						AS INT
            DECLARE @CodigoEntidadeMapa	                AS INT
            DECLARE @CodigoUnidadeMapa	                AS INT

            SET @CodigoMapaEstrategico = {2}
            SET @CodigoEntidadLogada = {3}
            SET @CodigoUsuario = {4}

            SELECT @CodigoEntidadeMapa = un.CodigoEntidade 
                ,  @CodigoUnidadeMapa  = un.CodigoUnidadeNegocio
            FROM        MapaEstrategico me 
            INNER JOIN	UnidadeNegocio un ON un.CodigoUnidadeNegocio = me.CodigoUnidadeNegocio
            WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico

            -- PRENCHO A TABELA.
	        INSERT INTO @tbResumo
			    SELECT CONVERT(VarChar, CodigoMapaEstrategico), 
			           null, 
			           DescricaoObjetoEstrategia, 
			           null, 
			           'MAP',
			           1
				    FROM {0}.{1}.ObjetoEstrategia
			     WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico
			       AND CodigoTipoObjetoEstrategia IN(SELECT CodigoTipoObjetoEstrategia 
                                                 FROM {0}.{1}.TipoObjetoEstrategia 
                                                WHERE IniciaisTipoObjeto = 'MAP'
                                                  AND DataExclusao IS NULL)
             AND DataExclusao IS NULL
        			   
	        INSERT INTO @tbResumo
		        SELECT CONVERT(VarChar, CodigoObjetoEstrategia), 
		               CONVERT(VarChar, CodigoMapaEstrategico), 
		               TituloObjetoEstrategia, 
		               null, 
		               'PSP',
			           1
				      FROM {0}.{1}.ObjetoEstrategia
				     WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico
				       AND CodigoTipoObjetoEstrategia IN(SELECT CodigoTipoObjetoEstrategia 
                                                   FROM {0}.{1}.TipoObjetoEstrategia 
                                                  WHERE IniciaisTipoObjeto = 'PSP'
                                                    AND DataExclusao IS NULL)
                  AND DataExclusao IS NULL
				ORDER BY OrdemObjeto DESC
        	
        	 INSERT INTO @tbResumo
		        SELECT CONVERT(VarChar, CodigoObjetoEstrategia), 
		               CONVERT(VarChar, CodigoObjetoEstrategiaSuperior), 
		               TituloObjetoEstrategia, 
		               {0}.{1}.f_GetCorTema(@CodigoEntidadLogada, CodigoObjetoEstrategia, YEAR(GETDATE()), MONTH(GETDATE())), 
		               'TEM',
			           1 
				      FROM {0}.{1}.ObjetoEstrategia
				     WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico
				       AND CodigoTipoObjetoEstrategia IN(SELECT CodigoTipoObjetoEstrategia 
                                                   FROM {0}.{1}.TipoObjetoEstrategia 
                                                  WHERE IniciaisTipoObjeto = 'TEM'
                                                    AND DataExclusao IS NULL)
                  AND DataExclusao IS NULL
				ORDER BY OrdemObjeto DESC
        						
	        INSERT INTO @tbResumo
		           SELECT  CONVERT(VarChar, CodigoObjetoEstrategia), 
		                   CONVERT(VarChar, CodigoObjetoEstrategiaSuperior), 
		                   ISNULL(TituloObjetoEstrategia, DescricaoObjetoEstrategia) as DescricaoObjetoEstrategia,
                           {0}.{1}.f_GetCorObjetivo(@CodigoEntidadLogada, CodigoObjetoEstrategia, YEAR(GETDATE()), MONTH(GETDATE())), 
                          'OBJ',
                          {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidadLogada, CodigoObjetoEstrategia, NULL, 'OB', 0, NULL, 'OB_CnsDtl')
				          FROM {0}.{1}.ObjetoEstrategia
				         WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico
				           AND CodigoTipoObjetoEstrategia IN(SELECT CodigoTipoObjetoEstrategia 
                                                       FROM {0}.{1}.TipoObjetoEstrategia 
                                                      WHERE IniciaisTipoObjeto = 'OBJ'
                                                        AND DataExclusao IS NULL)
				           AND CodigoObjetoEstrategiaSuperior IN (SELECT Codigo FROM @tbResumo)
                   AND DataExclusao IS NULL
				ORDER BY OrdemObjeto
        						 
            -- se o mapa estiver ligado a uma unidade, pega os resultados dos indicadores nessa unidade, de preferência. 
            -- para os que não tiverem resultados na unidade, pega da entidade
			IF (@CodigoEntidadeMapa != @CodigoUnidadeMapa)
			BEGIN
	            INSERT INTO @tbResumo
		            SELECT      'I' + CONVERT(VarChar, i.CodigoIndicador) + '.' + CONVERT(VarChar, ioe.CodigoObjetivoEstrategico)
                            ,   CONVERT(VarChar, ioe.CodigoObjetivoEstrategico)
                            ,   i.NomeIndicador
                            ,   {0}.{1}.f_GetUltimoDesempenhoIndicador(@CodigoUnidadeMapa, i.CodigoIndicador, YEAR(GETDATE()), MONTH(GETDATE()), 'A')
                            , 'IND'
                            , {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidadLogada, i.[CodigoIndicador], NULL, 'IN', @CodigoMapaEstrategico*(-1), NULL, 'IN_CnsDtl')
		            FROM        {0}.{1}.Indicador i 
                    INNER JOIN  {0}.{1}.IndicadorUnidade iu ON iu.CodigoIndicador = i.CodigoIndicador AND iu.CodigoUnidadeNegocio = @CodigoUnidadeMapa AND iu.DataExclusao IS NULL
                    INNER JOIN  {0}.{1}.IndicadorObjetivoEstrategico ioe ON ioe.CodigoIndicador = i.CodigoIndicador	

		            WHERE i.DataExclusao IS NULL
			          AND CONVERT(VarChar,ioe.CodigoObjetivoEstrategico) IN(SELECT Codigo FROM @tbResumo)
	 
	        END  -- IF (@CodigoEntidadeMapa != @CodigoUnidadeMapa)

	        INSERT INTO @tbResumo
		        SELECT      'I' + CONVERT(VarChar, i.CodigoIndicador) + '.' + CONVERT(VarChar, ioe.CodigoObjetivoEstrategico)
                        ,   CONVERT(VarChar, ioe.CodigoObjetivoEstrategico)
                        ,   i.NomeIndicador
                        ,   {0}.{1}.f_GetUltimoDesempenhoIndicador(@CodigoEntidadeMapa, i.CodigoIndicador, YEAR(GETDATE()), MONTH(GETDATE()), 'A')
                        , 'IND'
                        , {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidadLogada, i.[CodigoIndicador], NULL, 'IN', @CodigoMapaEstrategico*(-1), NULL, 'IN_CnsDtl')
		        FROM        {0}.{1}.Indicador i 
                INNER JOIN  {0}.{1}.IndicadorObjetivoEstrategico ioe ON ioe.CodigoIndicador = i.CodigoIndicador	

		        WHERE i.DataExclusao IS NULL
			        AND CONVERT(VarChar,ioe.CodigoObjetivoEstrategico) IN(SELECT Codigo FROM @tbResumo)	
                    AND 'I' + CONVERT(VarChar, i.CodigoIndicador) + '.' + CONVERT(VarChar, ioe.CodigoObjetivoEstrategico) NOT IN(SELECT Codigo FROM @tbResumo)	 

		    IF @CodigoEntidadLogada = @CodigoEntidadeMapa
			BEGIN
                       INSERT INTO @tbResumo
                        SELECT	  'P' + CONVERT(VarChar, rp.CodigoProjeto) + '.' + CONVERT(VarChar, poe.CodigoObjetivoEstrategico)
						        , CONVERT(VarChar, poe.CodigoObjetivoEstrategico)
						        , p.NomeProjeto
						        , RTRIM(LTRIM(rp.CorGeral))
						        , 'INI'	
                                , {0}.{1}.f_VerificaAcessoConcedido(@CodigoUsuario, @CodigoEntidadLogada, p.CodigoProjeto, NULL, 'PR', 0, NULL, 'PR_Acs')	        
                        FROM        {0}.{1}.ResumoProjeto rp 
                        INNER JOIN  {0}.{1}.Projeto p ON p.CodigoProjeto = rp.CodigoProjeto
                         LEFT JOIN  {0}.{1}.Usuario u ON u.CodigoUsuario = p.CodigoGerenteProjeto
                        INNER JOIN  {0}.{1}.ProjetoObjetivoEstrategico poe ON poe.CodigoProjeto = rp.CodigoProjeto
                         LEFT JOIN  {0}.{1}.ParametroConfiguracaoSistema AS pcs ON (pcs.Parametro = 'MostraProjetoEncerradoObjetivoEstrategico' and pcs.CodigoEntidade = p.CodigoEntidade)
                        WHERE  p.CodigoEntidade = @CodigoEntidadLogada
                               AND (pcs.Valor = 'S' OR p.CodigoStatusProjeto <> 6)
                               AND p.DataExclusao IS NULL
                               AND U.CodigoUsuarioExclusao IS NULL
				               AND CONVERT(VarChar,poe.CodigoObjetivoEstrategico) IN(SELECT Codigo FROM @tbResumo)	 
            END

            SELECT * FROM @tbResumo WHERE 1 = 1 {5}
        END
        
        ", bancodb, Ownerdb, codigoMapaEstrategico, codigoEntidade, codigoUsuario, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getObjetosMapaEstrategico(int codigoMapaEstrategico, int codigoEntidade, char desempenho, int mes, int ano, int codigoUsuarioLogado, string where)
    {
        comandoSQL = string.Format(
             @"
             BEGIN
	                DECLARE @AnoAtual INT,
				            @MesAtual INT
                				  
	                SET @AnoAtual = {5}
	                SET @MesAtual = {6}

              EXEC {0}.{1}.p_GetDesempenhoMapaEstrategico {2}, {3}, @AnoAtual, @MesAtual, '{4}', {7}
            END", bancodb, Ownerdb, codigoMapaEstrategico, codigoEntidade, desempenho, ano, mes, codigoUsuarioLogado);

        return getDataSet(comandoSQL);
    }

    public DataSet getObjetivosEstrategicosMapa(int codigoMapaEstrategico, string where)
    {
        comandoSQL = string.Format(@"
                SELECT  oe.CodigoObjetoEstrategia
                    ,   oe.DescricaoObjetoEstrategia 

                 FROM {0}.{1}.ObjetoEstrategia oe INNER JOIN
                      {0}.{1}.TipoObjetoEstrategia toe ON toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia

                WHERE CodigoMapaEstrategico = {2}
                  AND DataExclusao          IS NULL 
                  AND toe.IniciaisTipoObjeto= 'OBJ'
                  {3}

                ORDER BY oe.DescricaoObjetoEstrategia", bancodb, Ownerdb, codigoMapaEstrategico, where);

        return getDataSet(comandoSQL);
    }

    /// <summary>
    /// Obter o estado de ativo do mapa estratégico indicado no banco.
    /// </summary>
    /// <param name="codigoMapaEstrategico"></param>
    /// <param name="ativoNoBanco"></param>
    public void getEstadoDoMapaEstrategicoNoBanco(int codigoMapaEstrategico, ref bool ativoNoBanco)
    {
        ativoNoBanco = false;
        string commandoSql = string.Format(@"
                SELECT IndicaMapaEstrategicoAtivo 
                FROM {0}.{1}.MapaEstrategico 
                WHERE CodigoMapaEstrategico = {2}
                ", bancodb, Ownerdb, codigoMapaEstrategico);

        DataSet ds = getDataSet(commandoSql);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            string resultado = ds.Tables[0].Rows[0]["IndicaMapaEstrategicoAtivo"].ToString();
            if (resultado.Equals("S"))
                ativoNoBanco = true;
        }
    }

    public bool atualizaMapaEstrategico(int codigoMapaEstrategico, string TituloMapaEstrategico
                                        , char MapaAtivo, int VersaoMapaEstrategico
                                        , int codigoUnidadeNegocio, string indicaMapaCarregado, int codigoEntidade)
    {
        int registrosAfectados = 0;
        bool retorno = false;

        try
        {
            string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoMapaEstrategico int
                DECLARE @MapaAtivo char(1)
                DECLARE @DataInicioVersao Datetime
                DECLARE @CodigoUnidadeNegocio int
                DECLARE @VersaoMapaEstrategico int
                DECLARE @IndicaMapaCarregado CHAR(1)

                SET @CodigoMapaEstrategico  = {2}
                SET @MapaAtivo              = '{4}'
                SET @VersaoMapaEstrategico  = {5}
                SET @CodigoUnidadeNegocio   = {6}
                SET @IndicaMapaCarregado    = '{7}'
--                SET @CodigoUnidadeNegocio   = (SELECT CodigoUnidadeNegocio 
--                                                 FROM {0}.{1}.MapaEstrategico 
--                                                WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico )

                if (@MapaAtivo = 'S')   --------------------------------
                    SET @DataInicioVersao = GetDate();

                UPDATE {0}.{1}.MapaEstrategico 
                   SET TituloMapaEstrategico        = '{3}'
                     , IndicaMapaEstrategicoAtivo   = @MapaAtivo
                     , CodigoUnidadeNegocio         = @CodigoUnidadeNegocio
                     , IndicaMapaCarregado          = @IndicaMapaCarregado
                 WHERE CodigoMapaEstrategico        = @CodigoMapaEstrategico

                UPDATE ObjetoEstrategia 
                   SET DescricaoObjetoEstrategia    = '{3}' 
                 WHERE CodigoMapaEstrategico        = @CodigoMapaEstrategico 
                   AND CodigoVersaoMapaEstrategico  = @VersaoMapaEstrategico 
                   AND CodigoTipoObjetoEstrategia   = 1

                if (@MapaAtivo = 'S')   --------------------------------
                BEGIN
                    --UPDATE {0}.{1}.MapaEstrategico
                    --   SET IndicaMapaEstrategicoAtivo = 'N'
                    -- WHERE CodigoUnidadeNegocio = @CodigoUnidadeNegocio
                    --   AND CodigoMapaEstrategico <> @CodigoMapaEstrategico

                   -- UPDATE {0}.{1}.VersaoMapaEstrategico
                   --    SET DataTerminoVersaoMapaEstrategico = GetDate()
                   --  WHERE DataTerminoVersaoMapaEstrategico is null
                   --   AND CodigoMapaEstrategico <> @CodigoMapaEstrategico
                   --    AND CodigoMapaEstrategico in (SELECT CodigoMapaEstrategico
                   --                                    FROM {0}.{1}.MapaEstrategico
                   --                                   WHERE CodigoUnidadeNegocio = @CodigoUnidadeNegocio )

                    UPDATE {0}.{1}.VersaoMapaEstrategico
                       SET DataTerminoVersaoMapaEstrategico = null
                         , DataInicioVersaoMapaEstrategico  = GetDate()
                     WHERE CodigoMapaEstrategico = @CodigoMapaEstrategico
                       AND VersaoMapaEstrategico = @VersaoMapaEstrategico
                END
                ELSE
                    UPDATE {0}.{1}.VersaoMapaEstrategico
                       SET DataTerminoVersaoMapaEstrategico = GetDate()
                     WHERE DataTerminoVersaoMapaEstrategico is null
                       AND CodigoMapaEstrategico = @CodigoMapaEstrategico
                       AND VersaoMapaEstrategico = @VersaoMapaEstrategico

            
                    --Em seguida à gravação dos dados, deve ser acionada a proc [p_ProcessaMatrizAcessos] 
                    --para que a matriz de acesso do usuário logado seja refeita para o mapa cujos dados estão sendo gravadas;
                    
                    DECLARE @RC int
                    DECLARE @in_iniciaisTipoObjeto char(2)
                        DECLARE @in_codigoObjetoPai bigint
                    DECLARE @in_codigoEntidade int
                    DECLARE @in_codigoPermissao smallint
                    DECLARE @in_codigoUsuario int
                    DECLARE @in_codigoPerfil int
                    DECLARE @in_codigoObjeto INT

                    SET @in_iniciaisTipoObjeto =  'ME'       
                    SET @in_codigoObjeto = @CodigoMapaEstrategico
                    SET @in_codigoObjetoPai = 0
                    SET @in_codigoEntidade = {8}
                    SET @in_codigoPermissao = NULL
                    SET @in_codigoUsuario = {5}
                    SET @in_codigoPerfil = NULL

                    EXECUTE @RC = [dbo].[p_ProcessaMatrizAcessos] 
                        @in_iniciaisTipoObjeto
                        ,@in_codigoObjeto
                        ,@in_codigoObjetoPai
                        ,@in_codigoEntidade
                        ,@in_codigoPermissao
                        ,@in_codigoUsuario
                        ,@in_codigoPerfil

                   --Em seguida ao acionamento da proc do item anterior, deve ser acionada outra proc, 
                   --a [p_incluiJobProcessaMatrizAcessos] para que seja incluído um job que referá a matriz de permissão do mapa para todos os usuários;

                    EXECUTE @RC = [dbo].[p_incluiJobProcessaMatrizAcessos] 
                       @in_iniciaisTipoObjeto
                      ,@in_codigoObjeto
                      ,@in_codigoObjetoPai
                      ,@in_codigoEntidade
                      ,@in_codigoPermissao
                      ,NULL
                      ,@in_codigoPerfil
                      ,NULL

                
            END
            ", bancodb, Ownerdb, codigoMapaEstrategico, TituloMapaEstrategico
             , MapaAtivo, VersaoMapaEstrategico, codigoUnidadeNegocio, indicaMapaCarregado, codigoEntidade);

            execSQL(comandoSQL, ref registrosAfectados);
            retorno = true;
        }
        catch (Exception ex)
        {
            throw new Exception(Delimitador_Erro + "Ocorreu um erro ao atualizar o registro.\n\n" + Delimitador_Erro + " \n " + ex.Message + "\n" + comandoSQL);
        }
        return retorno;
    }

    public DataSet getMatrizSuficiencia(int codigoEntidade, int codigoMapa)
    {
        string comandoSQL = string.Format(@"
        begin
            declare @CodigoEntidade as int
            set @CodigoEntidade = {2}
            declare @CodigoMapaEstrategico as int
            set @CodigoMapaEstrategico = {3}

            select uno.SiglaUnidadeNegocio AS UnidadeNegocio,
                   p.NomeProjeto AS NomeProjeto,                 
                   oe.DescricaoObjetoEstrategia AS ObjetivoEstrategico,
                   psp.TituloObjetoEstrategia AS Perspectiva,
                   u.NomeUsuario AS LiderProjeto,
                   CASE WHEN p.NomeProjeto IS NOT NULL THEN 1 ELSE Null END AS PossuiObjetivoAssociado,
                   rp.PercentualRealizacao AS PercentualConcluido,
                   s.DescricaoStatus AS StatusProjeto,
                   f.DescricaoFaseGP AS FaseProjeto,
                   null AS Tema,
                   uObj.NomeUsuario AS ResponsavelObjetivo,
                   null AS ResponsavelTema,
                   tp.TipoProjeto
             FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                  {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                      AND toe.IniciaisTipoObjeto = 'OBJ') INNER JOIN
                  {0}.{1}.ObjetoEstrategia AS psp ON (psp.CodigoObjetoEstrategia = oe.CodigoObjetoEstrategiaSuperior
                                                  AND psp.DataExclusao IS NULL) INNER JOIN
                  {0}.{1}.TipoObjetoEstrategia AS toepsp ON (toepsp.CodigoTipoObjetoEstrategia = psp.CodigoTipoObjetoEstrategia
                                                      AND toepsp.IniciaisTipoObjeto = 'PSP')LEFT JOIN
                  {0}.{1}.ProjetoObjetivoEstrategico AS poe ON (oe.CodigoObjetoEstrategia = poe.CodigoObjetivoEstrategico) LEFT JOIN
                  {0}.{1}.Projeto AS p ON (poe.CodigoProjeto = p.CodigoProjeto 
                               AND p.DataExclusao IS NULL
                               AND p.CodigoEntidade = @CodigoEntidade) LEFT JOIN 
                  {0}.{1}.ResumoProjeto AS rp ON (rp.CodigoProjeto = p.CodigoProjeto) LEFT JOIN
                  {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) LEFT JOIN
                  {0}.{1}.FaseGP AS f ON (f.CodigoFaseGP = s.CodigoFaseGP) LEFT JOIN             
                  {0}.{1}.UnidadeNegocio AS uno ON (uno.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN   
                  {0}.{1}.Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto)  LEFT JOIN
                  {0}.{1}.Usuario AS uObj ON (uObj.CodigoUsuario = oe.CodigoResponsavelObjeto) LEFT JOIN
                  {0}.{1}.TipoProjeto as tp ON (tp.CodigoTipoProjeto  = p.CodigoTipoProjeto)
            WHERE oe.CodigoMapaEstrategico = @CodigoMapaEstrategico
              AND oe.DataExclusao IS NULL
           UNION
           select uno.SiglaUnidadeNegocio AS UnidadeNegocio,
                   p.NomeProjeto AS NomeProjeto,                 
                   oe.DescricaoObjetoEstrategia AS ObjetivoEstrategico,
                   psp.TituloObjetoEstrategia AS Perspectiva,
                   u.NomeUsuario AS LiderProjeto,
                   CASE WHEN p.NomeProjeto IS NOT NULL THEN 1 ELSE Null END AS PossuiObjetivoAssociado,
                   rp.PercentualRealizacao AS PercentualConcluido,
                   s.DescricaoStatus AS StatusProjeto,
                   f.DescricaoFaseGP AS FaseProjeto,
                   tem.TituloObjetoEstrategia AS Tema,
                   uObj.NomeUsuario AS ResponsavelObjetivo,
                   uTem.NomeUsuario AS ResponsavelTema,
                   tp.TipoProjeto
             FROM {0}.{1}.ObjetoEstrategia AS oe INNER JOIN
                  {0}.{1}.TipoObjetoEstrategia AS toe ON (toe.CodigoTipoObjetoEstrategia = oe.CodigoTipoObjetoEstrategia
                                                      AND toe.IniciaisTipoObjeto = 'OBJ') INNER JOIN
                  {0}.{1}.ObjetoEstrategia AS tem ON (tem.CodigoObjetoEstrategia = oe.CodigoObjetoEstrategiaSuperior
                                                  AND tem.DataExclusao IS NULL) INNER JOIN                                    
                  {0}.{1}.ObjetoEstrategia AS psp ON (psp.CodigoObjetoEstrategia = tem.CodigoObjetoEstrategiaSuperior
                                                  AND psp.DataExclusao IS NULL) INNER JOIN
                  {0}.{1}.TipoObjetoEstrategia AS toepsp ON (toepsp.CodigoTipoObjetoEstrategia = psp.CodigoTipoObjetoEstrategia
                                                      AND toepsp.IniciaisTipoObjeto = 'PSP')LEFT JOIN
                  {0}.{1}.ProjetoObjetivoEstrategico AS poe ON (oe.CodigoObjetoEstrategia = poe.CodigoObjetivoEstrategico) LEFT JOIN
                  {0}.{1}.Projeto AS p ON (poe.CodigoProjeto = p.CodigoProjeto 
                               AND p.DataExclusao IS NULL
                               AND p.CodigoEntidade = @CodigoEntidade) LEFT JOIN 
                  {0}.{1}.ResumoProjeto AS rp ON (rp.CodigoProjeto = p.CodigoProjeto) LEFT JOIN
                  {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) LEFT JOIN
                  {0}.{1}.FaseGP AS f ON (f.CodigoFaseGP = s.CodigoFaseGP) LEFT JOIN             
                  {0}.{1}.UnidadeNegocio AS uno ON (uno.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN   
                  {0}.{1}.Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) LEFT JOIN
                  {0}.{1}.Usuario AS uObj ON (uObj.CodigoUsuario = oe.CodigoResponsavelObjeto) LEFT JOIN
                  {0}.{1}.Usuario AS uTem ON (uTem.CodigoUsuario = tem.CodigoResponsavelObjeto)  LEFT JOIN
                  {0}.{1}.TipoProjeto as tp ON (tp.CodigoTipoProjeto  = p.CodigoTipoProjeto)   
            WHERE oe.CodigoMapaEstrategico = @CodigoMapaEstrategico
              AND oe.DataExclusao IS NULL   
   end
", bancodb, Ownerdb, codigoEntidade, codigoMapa);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Grafico Processo

    public DataSet getInstanciasWF(string codigoInstanciaWorkflow, string codigoWorkflow, int codigoEntidade)
    {
        comandoSQL = string.Format(@"
            -- ---Tabela 0
            SELECT  
                 eiw.CodigoEtapaWf
                ,aew.[TextoAcao]
                ,eiw.[CodigoAcaoWfFinalizacaoEtapa]

		    FROM    
			    {0}.{1}.EtapasInstanciasWf AS eiw
				    LEFT JOIN {0}.{1}.[AcoesEtapasWf] AS aew
						    ON (	aew.[CodigoWorkflow] = eiw.[CodigoWorkflow]
							    AND aew.[CodigoEtapaWf]  = eiw.[CodigoEtapaWf]
							    AND aew.[CodigoAcaoWf]   = eiw.[CodigoAcaoWfFinalizacaoEtapa] )
		    WHERE   
                    eiw.[CodigoInstanciaWf] = {2} 
                AND eiw.[CodigoWorkflow]    = {3}
                AND (eiw.[DataTerminoEtapa] IS NULL OR eiw.[CodigoAcaoWfFinalizacaoEtapa] IS NOT NULL);

            -- ---Tabela 1
            SELECT  
			      iw.[EtapaAtual]
		        , iw.[CodigoWorkflow]
                , iw.[NomeInstancia]
                , iw.[NumeroProtocolo]
                , iw.[OcorrenciaAtual]
		    FROM    
			    {0}.{1}.[InstanciasWorkflows] AS iw
		    WHERE   
			        iw.[CodigoInstanciaWf] = {2}  
			    AND iw.[CodigoWorkflow] = {3};

            -- ---Tabela 2
            SELECT  w.[TextoXML]
                  , f.[NomeFluxo] AS NomeWorkflow
		    FROM  {0}.{1}.[Workflows] AS w INNER JOIN {0}.{1}.[Fluxos] AS f ON 
                    ( f.[CodigoFluxo] = w.[CodigoFluxo] )
		    WHERE w.[CodigoWorkflow] = {3};

            -- ---Tabela 3 [Configurações
            SELECT Parametro, Valor
            FROM {0}.{1}.ParametroConfiguracaoSistema
            WHERE  Parametro IN ( 'corFundoEtapaAtual_HistoricoWF', 
                'corFundoEtapaPercorrida_HistoricoWf', 
                'corFundoEtapaNaoPercorrida_HistoricoWf',
                'corFonte_HistoricoWf',
                'corLinhaConectorPercorrido_HistoricoWf',
                'corLinhaConectorNaoPercorrido_HistoricoWf')
            AND CodigoEntidade = {4};
					", bancodb, Ownerdb, codigoInstanciaWorkflow, codigoWorkflow, codigoEntidade);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region Histórico Processo

    public DataSet getInstanciaWf(int codigoWorkflow, int codigoInstanciaWorkflow)
    {
        comandoSQL = string.Format(@"
SELECT 
      i.[NomeInstancia]             AS [NomeInstancia]
    , i.[DataInicioInstancia]       AS [DataInicioInstancia]
    , i.[DataTerminoInstancia]      AS [DataTerminoInstancia]
    , i.[DataCancelamentoInstancia] AS [DataCancelamentoInstancia]
    , e.[NomeEtapaWf]               AS [EtapaAtual]
    , CAST (CASE 
            WHEN i.[DataCancelamentoInstancia] IS NOT NULL THEN 'Cancelado' 
            WHEN i.[DataTerminoInstancia] IS NULL THEN 'Ativo' 
            ELSE 'Finalizado' END AS Varchar(15) ) AS [Status]

    FROM 
        {0}.{1}.[InstanciasWorkflows]       AS [i]

            LEFT JOIN {0}.{1}.EtapasWf      AS [e]
                ON (    e.[CodigoWorkflow]  = i.[CodigoWorkflow] 
                    AND e.[CodigoEtapaWf]   = i.[EtapaAtual] )
    WHERE   
            i.[CodigoWorkflow] = {2} 
        AND i.[CodigoInstanciaWf] = {3} ", bancodb, Ownerdb, codigoWorkflow, codigoInstanciaWorkflow);

        return getDataSet(comandoSQL);

    }

    public DataSet getHistoricoInstanciaWf(int codigoWorkflow, int codigoInstanciaWorkflow)
    {
        comandoSQL = string.Format(@"
    SELECT 
		  [CodigoWorkflow]
        , [CodigoInstanciaWf]
		, [SequenciaOcorrenciaEtapaWf]
        , [CodigoEtapaWf]
		, [NomeEtapaWf]
		, [DataInicioEtapa]
        , [TerminoPrevisto]
		, [DataTerminoEtapa]
		, [NomeUsuarioFinalizador]
		, [TextoAcao]
        , [CodigoAcaoWfFinalizacaoEtapa]
		, [TempoDecorrido]
		, [ComAtraso]
        , [Atraso]
        , [IndicaEtapaAtual]
        , [IndicaInicioSubWorkflow]
        , [IndicaEtapaDecisao]
        , [CodigoFluxoSubprocesso]
	    , [CodigoWorkflowSubprocesso]
	    , [CodigoInstanciaSubprocesso]
	FROM 
		[{0}].[{1}].[f_wf_historicoInstanciaWorkflow]({2}, {3})  
	ORDER BY 
		[SequenciaOcorrenciaEtapaWf]"
                                    , bancodb, Ownerdb, codigoWorkflow, codigoInstanciaWorkflow);
        return getDataSet(comandoSQL);
    }

    public DataSet getGruposFluxos(int codigoEntidade)
    {
        comandoSQL = string.Format(@"
    SELECT * FROM {0}.{1}.GrupoFluxo
     WHERE CodigoEntidade = {2}  
		  ", bancodb, Ownerdb, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaFluxosGrupo(int codigoEntidade, int codigoGrupo)
    {
        comandoSQL = string.Format(@"
    SELECT CodigoFluxo, NomeFluxo, Descricao 
      FROM {0}.{1}.fluxos AS f 
     WHERE f.CodigoEntidade = {2}
       AND f.CodigoGrupoFluxo = {3}
       AND EXISTS (SELECT 1 FROM {0}.{1}.InstanciasWorkflows AS iw INNER JOIN 
                                 {0}.{1}.Workflows AS w ON (iw.CodigoWorkflow = w.codigoworkflow 
                                                        AND w.codigofluxo = f.codigofluxo))
    ORDER BY NomeFluxo
		  ", bancodb, Ownerdb, codigoEntidade, codigoGrupo);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region Menu Administração

    #region SUB-menu Configuração (fluxo caixa).

    public DataSet getGridPeriodicidade(string codigoEntidadeUsuarioResponsavel)
    {
        string comandoSQL = string.Format(@"
        SELECT  pa.CodigoEntidade, pa.Ano, 
                CASE WHEN pa.IndicaAnoPeriodoEditavel = 'N' THEN '" + Resources.traducao.n_o + @"'
                ELSE '" + Resources.traducao.sim + @"' END AS IndicaAnoPeriodoEditavel, 
                tp.DescricaoPeriodicidade_PT as IndicaTipoDetalheEdicao,

                    CASE IndicaAnoAtivo WHEN 'S' THEN '" + Resources.traducao.sim + @"'
                                        WHEN 'N' THEN '" + Resources.traducao.n_o + @"' END AS AnoAtivo, 
                    CASE IndicaMetaEditavel WHEN 'S' THEN '" + Resources.traducao.sim + @"'
                                        WHEN 'N' THEN '" + Resources.traducao.n_o + @"' END AS MetaEditavel,
                    CASE IndicaResultadoEditavel WHEN 'S' THEN '" + Resources.traducao.sim + @"'
                                        WHEN 'N' THEN '" + Resources.traducao.n_o + @"' END AS ResultadoEditavel,
                    IndicaAnoAtivo, 
                    IndicaMetaEditavel,
                    IndicaResultadoEditavel,
                    tp.CodigoPeriodicidade

        FROM    {0}.{1}.PeriodoAnalisePortfolio As pa
        LEFT JOIN {0}.{1}.TipoPeriodicidade tp on (tp.CodigoPeriodicidade = pa.CodigoPeriodicidade) 
        WHERE   pa.CodigoEntidade = {2}", bancodb, Ownerdb, codigoEntidadeUsuarioResponsavel);
        return getDataSet(comandoSQL);
    }

    public DataSet getRelLogAcessoSistema(int codigoUsuarioLogado, int codigoEntidade, string txtInicio, string txtTermino)
    {
        string comandoSQL = string.Format(@"
        DECLARE
		    @DataInicial		DateTime
	       ,@DataFinal			DateTime
            
            SET @DataInicial	= CONVERT(datetime,'{4}',103)
            SET @DataFinal		= CONVERT(datetime,'{5}',103)
            SET @DataFinal		= DATEADD(DD,1,@DataFinal)
            
            SELECT NomeUsuario,
                   Convert(Varchar,DataAcesso,108) AS Horario,
                   CONVERT(Varchar,DataAcesso,103) AS Data,       
                   Year(DataAcesso) As Ano,
                   CASE Month(DataAcesso) WHEN   1 THEN 'Janeiro'
                        WHEN 2 THEN 'Fevereiro'
                        WHEN 3 THEN 'Março'
                        WHEN 4 THEN 'Abril'
                        WHEN 5 THEN 'Maio'
                        WHEN 6 THEN 'Junho'
                        WHEN 7 THEN 'Julho'
                        WHEN 8 THEN 'Agosto'
                        WHEN 9 THEN 'Setembro'
                        WHEN 10 THEN 'Outubro'
                        WHEN 11 THEN 'Novembro'
                        WHEN 12 THEN 'Dezembro' 
                  END as Mes,
                  1 AS QuantidadeAcesso  
            FROM  {0}.{1}.f_GetDadosOLAP_LogAcessosSistema({2},{3})
           WHERE DataAcesso >= @DataInicial
	         AND DataAcesso < @DataFinal", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, txtInicio, txtTermino);
        return getDataSet(comandoSQL);
    }

    public DataSet getRelLogAtualizacaoInformacoes(int codigoUsuarioLogado, int codigoEntidade, string txtInicio, string txtTermino)
    {
        string comandoSQL = string.Format(@"
        DECLARE
		    @DataInicial		DateTime
	       ,@DataFinal			DateTime
            
            SET @DataInicial	= CONVERT(datetime,'{4}',103)
            SET @DataFinal		= CONVERT(datetime,'{5}',103)
            SET @DataFinal		= DATEADD(DD,1,@DataFinal)
            
            SELECT NomeUsuario,
            case 
            when TipoOperacao = 'I' then 'Inclusão'
            when TipoOperacao = 'E'then 'Exclusão'
            when TipoOperacao = 'A'then 'Alteração' 
            end as TipoOperacao,
            DataAlteracao,Tabela,Campo,InformacaoAnterior,NovaInformacao,
            Convert(Varchar,DataAlteracao,108) AS Horario,
            CONVERT(Varchar,DataAlteracao,103) AS Data,       
           Year(DataAlteracao) As Ano,
           CASE WHEN Month(DataAlteracao) = 1 THEN 'Janeiro'
                WHEN Month(DataAlteracao) = 2 THEN 'Fevereiro'
                WHEN Month(DataAlteracao) = 3 THEN 'Março'
                WHEN Month(DataAlteracao) = 4 THEN 'Abril'
                WHEN Month(DataAlteracao) = 5 THEN 'Maio'
                WHEN Month(DataAlteracao) = 6 THEN 'Junho'
                WHEN Month(DataAlteracao) = 7 THEN 'Julho'
                WHEN Month(DataAlteracao) = 8 THEN 'Agosto'
                WHEN Month(DataAlteracao) = 9 THEN 'Setembro'
                WHEN Month(DataAlteracao) = 10 THEN 'Outubro'
                WHEN Month(DataAlteracao) = 11 THEN 'Novembro'
                WHEN Month(DataAlteracao) = 12 THEN 'Dezembro' 
           END as Mes,             
           1 AS QuantidadeAcesso, IdentificadorRegistro
           FROM {0}.{1}.[f_GetDadosOLAP_LogAlteracoesSistema]({2},{3})  
             WHERE DataAlteracao >= @DataInicial
	           AND DataAlteracao < @DataFinal", bancodb, Ownerdb, codigoUsuarioLogado, codigoEntidade, txtInicio, txtTermino);
        return getDataSet(comandoSQL);
    }


    #endregion

    #region ramo de Atividade
    public DataSet getTipoRamoAtividade(string where)
    {

        comandoSQL = string.Format(@"
            SELECT tra.CodigoRamoAtividade, tra.RamoAtividade
              FROM {0}.{1}.TipoRamoAtividade tra
             WHERE 1 = 1
              {2}
            ORDER BY tra.RamoAtividade", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiTipoRamoAtividade(string ramoAtividade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoRamoAtividade int

                            INSERT INTO {0}.{1}.TipoRamoAtividade(RamoAtividade) 
                                 VALUES ('{2}')

                            SELECT @CodigoRamoAtividade = SCOPE_IDENTITY()

                            SELECT @CodigoRamoAtividade AS CodigoRamoAtividade

                          END
                        ", bancodb, Ownerdb
                         , ramoAtividade.Replace("'", "''"));

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoRamoAtividade"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoRamoAtividade(int codigoRamoAtividade, string ramoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoRamoAtividade int

                            SET @CodigoRamoAtividade = {2}
                            
                            UPDATE {0}.{1}.TipoRamoAtividade SET RamoAtividade = '{3}'
                             WHERE CodigoRamoAtividade = @CodigoRamoAtividade 
                        END
                        ", bancodb, Ownerdb
                         , codigoRamoAtividade
                         , ramoAtividade.Replace("'", "''"));

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

    public bool excluiTipoRamoAtividade(int codigoRamoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoRamoAtividade int

                            SET @CodigoRamoAtividade = {2}

                            DELETE FROM {0}.{1}.TipoRamoAtividade
                             WHERE CodigoRamoAtividade = @CodigoRamoAtividade                             
                        END
                        ", bancodb, Ownerdb
                         , codigoRamoAtividade);

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

    public bool verificaExclusaoTipoRamoAtividade(int codigoRamoAtividade)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Pessoa p
                       WHERE p.CodigoRamoAtividade = {2}
                        ", bancodb, Ownerdb
                         , codigoRamoAtividade);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region Categorias de Mensagens

    public DataSet getCategoriasMensagem(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
        SELECT [CodigoCategoria]
              ,[DescricaoCategoria]
              ,[IniciaisCategoriaControladaSistema]
              ,[CodigoEntidade]
        FROM {0}.{1}.[CategoriaMensagem]
        WHERE CodigoEntidade = {2}  {3}
        order by DescricaoCategoria", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiCategoriaMensagem(string descricaoCategoria, string iniciaisCategoriaControladaSistema, int codigoEntidade, ref int novoCodigo, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoCategoriaMensagem int

                           INSERT INTO {0}.{1}.CategoriaMensagem
                           (DescricaoCategoria,IniciaisCategoriaControladaSistema,CodigoEntidade)
                           VALUES
                           ('{2}',{3},{4})
                            SELECT @CodigoCategoriaMensagem = SCOPE_IDENTITY()

                            SELECT @CodigoCategoriaMensagem AS CodigoCategoriaMensagem

                          END
                        ", bancodb, Ownerdb
                         , descricaoCategoria.Replace("'", "''")
                         , iniciaisCategoriaControladaSistema
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoCategoriaMensagem"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaCategoriaMensagem(string descricaoCategoria, string iniciaisCategoriaControladaSistema, int codigoCategoria, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCategoriaMensagem int
                            DECLARE @DescricaoCategoriaMensagem varchar(100)
                            DECLARE @IniciaisCategoriaControladaSistema varchar(25)

                            SET @DescricaoCategoriaMensagem = '{2}'
                            SET @IniciaisCategoriaControladaSistema = '{3}'
                            SET @CodigoCategoriaMensagem = {4}

                            UPDATE {0}.{1}.CategoriaMensagem
                            SET DescricaoCategoria = @DescricaoCategoriaMensagem
                            ,IniciaisCategoriaControladaSistema = @IniciaisCategoriaControladaSistema
                            WHERE CodigoCategoria = @CodigoCategoriaMensagem
                        END", bancodb, Ownerdb
                         , descricaoCategoria
                         , iniciaisCategoriaControladaSistema
                         , codigoCategoria);

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

    public bool excluiCategoriaMensagem(int codigoCategoriaMensagem, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCategoria int

                            SET @CodigoCategoria = {2}

                            DELETE FROM {0}.{1}.CategoriaMensagem
                            WHERE CodigoCategoria = @CodigoCategoria
                        END", bancodb, Ownerdb
                         , codigoCategoriaMensagem);

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

    public bool verificaExclusaoCategoriaMensagem(int codigoCategoriaMensagem)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Mensagem m
                       WHERE m.CodigoCategoria = {2}
                        ", bancodb, Ownerdb
                         , codigoCategoriaMensagem);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region Tipo de Pagamento
    public bool verificaExclusaoTipoPagamento(int codigoTipoPagamento)
    {
        return true;
    }

    public DataSet getComboPagamentos(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
        SELECT -1 as CodigoTipoPagamentos, 'Não se aplica' as DescricaoTipoPagamentos
        union
        SELECT CodigoTipoPagamentos
               ,DescricaoTipoPagamentos
          FROM {0}.{1}.TipoPagamentos
          WHERE DataExclusao is null {3}", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getTipoPagamentos(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
        SELECT CodigoTipoPagamentos
               ,DescricaoTipoPagamentos
               ,CodigoEntidade
               ,DataExclusao
          FROM {0}.{1}.TipoPagamentos
          WHERE DataExclusao is null {3}", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public bool excluiTipoPagamento(int codigoTipoPagamento)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @codigoTipoPagamento int

                            SET @codigoTipoPagamento = {2}

                            UPDATE {0}.{1}.TipoPagamentos
                             SET DataExclusao = GetDate()
                            WHERE CodigoTipoPagamentos = @codigoTipoPagamento                             
                        END", bancodb, Ownerdb, codigoTipoPagamento);
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

    public bool incluiTipoPagamento(string descricaoTipoPagamento, int codigoEntidade, ref int novoCodigo, ref string mensagemErro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoTipoPagamentos int

                            INSERT INTO {0}.{1}.TipoPagamentos(DescricaoTipoPagamentos,CodigoEntidade) 
                                 VALUES ('{2}',{3})

                            SELECT @CodigoTipoPagamentos = SCOPE_IDENTITY()

                            SELECT @CodigoTipoPagamentos AS CodigoTipoPagamentos

                          END
                        ", bancodb, Ownerdb
                         , descricaoTipoPagamento.Replace("'", "''"), codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoTipoPagamentos"].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoPagamentos(int codigoTipoPagamento, string descricaoTipoPagamento)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @codigoTipoPagamento int

                            SET @codigoTipoPagamento = {2}
                            
                            UPDATE {0}.{1}.TipoPagamentos SET DescricaoTipoPagamentos = '{3}'
                             WHERE CodigoTipoPagamentos = @codigoTipoPagamento 
                        END
                        ", bancodb, Ownerdb
                         , codigoTipoPagamento
                         , descricaoTipoPagamento.Replace("'", "''"));

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

    #region Tipos de Obrigações da Contratada
    public DataSet getTipoObrigacoesContratada(string where)
    {
        comandoSQL = string.Format(@"
        SELECT CodigoTipoObrigacoesContratada
              ,DescricaoTipoObrigacoesContratada
         FROM {0}.{1}.TipoObrigacoesContratada
         where 1=1 {2}", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool excluiTipoObrigacoesContratada(int codigoTipoObrigacoesContratada)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @codigoTipoObrigacoesContratada int
                            SET @codigoTipoObrigacoesContratada = {2}
                            DELETE 
                              FROM {0}.{1}.TipoObrigacoesContratada
                             WHERE CodigoTipoObrigacoesContratada = @codigoTipoObrigacoesContratada
                        END", bancodb, Ownerdb, codigoTipoObrigacoesContratada);
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

    public bool incluiTipoObrigacoesContratada(string descricaoTipoObrigacoesContratada, ref string mensagemErro)
    {

        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                            INSERT INTO {0}.{1}.[TipoObrigacoesContratada](DescricaoTipoObrigacoesContratada)
                                         VALUES ('{2}')", bancodb, Ownerdb
                         , descricaoTipoObrigacoesContratada.Replace("'", "''"));
            int regAf = 0;
            execSQL(comandoSQL, ref regAf);
            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoObrigacoesContratada(int codigoTipoObrigacoesContratada, string descricaoTipoObrigacoesContratada)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @codigoTipoObrigacoesContratada int

                            SET @codigoTipoObrigacoesContratada = {2}
                            
                            UPDATE {0}.{1}.TipoObrigacoesContratada SET DescricaoTipoObrigacoesContratada = '{3}'
                             WHERE CodigoTipoObrigacoesContratada = @codigoTipoObrigacoesContratada 
                        END
                        ", bancodb, Ownerdb
                         , codigoTipoObrigacoesContratada
                         , descricaoTipoObrigacoesContratada.Replace("'", "''"));

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

    #region Fornecedores
    public DataSet getDadosPessoa(int codigoPessoa, string where)
    {

        comandoSQL = string.Format(@"
            SELECT p.CodigoPessoa, p.CodigoMunicipioEnderecoPessoa, p.EnderecoPessoa
	              ,p.InformacaoContato, p.NomeContato, p.NomePessoa, p.NumeroCNPJCPF
	              ,p.TelefonePessoa, p.TipoPessoa, m.NomeMunicipio + ' - ' + m.SiglaUF AS MunicipioFornecedor
             FROM {0}.{1}.Pessoa p INNER JOIN
                  {0}.{1}.Municipio m ON m.CodigoMunicipio = p.CodigoMunicipioEnderecoPessoa 
            WHERE p.CodigoPessoa = {2} 
              {3}", bancodb, Ownerdb, codigoPessoa, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getFornecedores(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT p.CodigoPessoa, p.CodigoMunicipioEnderecoPessoa, p.EnderecoPessoa
	              ,p.InformacaoContato, p.NomeContato, p.NomePessoa, p.NumeroCNPJCPF
	              ,p.TelefonePessoa, p.TipoPessoa, p.CodigoRamoAtividade, p.Email, p.Comentarios
                  ,p.NomeFantasia, m.NomeMunicipio, tra.RamoAtividade, m.SiglaUF, pe.IndicaCliente, pe.IndicaFornecedor, pe.IndicaParticipe
                  ,CASE WHEN (select COUNT(1) from Contrato c where c.CodigoPessoaContratada = p.CodigoPessoa and c.TipoPessoa = 'F') > 0 THEN 'S' ELSE 'N' END as TemContratoFornecedor
                  ,CASE WHEN (select COUNT(1) from Contrato c where c.CodigoPessoaContratada = p.CodigoPessoa and c.TipoPessoa = 'C') > 0 THEN 'S' ELSE 'N' END as TemContratoCliente
             FROM {0}.{1}.Pessoa p INNER JOIN 
                  {0}.{1}.PessoaEntidade pe ON pe.CodigoPessoa = p.CodigoPessoa LEFT JOIN
                  {0}.{1}.Municipio m ON m.CodigoMunicipio = p.CodigoMunicipioEnderecoPessoa LEFT JOIN
                  {0}.{1}.TipoRamoAtividade tra ON tra.CodigoRamoAtividade = p.CodigoRamoAtividade
            WHERE pe.CodigoEntidade = {2} 
              AND pe.IndicaPessoaAtivaEntidade = 'S'
              {3}
            ORDER BY p.NomePessoa", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }


    public bool incluiFornecedor(int codigoEntidade, string nomePessoa, string nomeFantasia, string tipoPessoa, string numeroCNPJCPF, string codigoRamoAtividade, string enderecoPessoa, string telefonePessoa
        , string nomeContato, string email, string informacaoContato, string comentarios, int codigoMunicipio, string indicaCliente, string indicaFornecedor, string indicaParticipe, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            if (numeroCNPJCPF == "")
                numeroCNPJCPF = "NULL";
            else
                numeroCNPJCPF = "'" + numeroCNPJCPF + "'";

            if (telefonePessoa.Replace("(", "").Replace(")", "").Replace("-", "").Trim() == "")
                telefonePessoa = "NULL";
            else
                telefonePessoa = "'" + telefonePessoa + "'";

            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoPessoa int

                            INSERT INTO {0}.{1}.Pessoa(NomePessoa, TipoPessoa, NumeroCNPJCPF, EnderecoPessoa, TelefonePessoa, NomeContato
                                                     , InformacaoContato, CodigoMunicipioEnderecoPessoa, codigoRamoAtividade, Email, Comentarios, NomeFantasia) 
                                 VALUES ('{2}', '{3}', {4}, '{5}', {6}, '{7}', '{8}', {9}, {11}, {12}, '{13}', '{14}')

                            SELECT @CodigoPessoa = SCOPE_IDENTITY()

                            INSERT INTO {0}.{1}.PessoaEntidade(CodigoPessoa, CodigoEntidade, IndicaCliente, IndicaFornecedor, IndicaParticipe, IndicaPessoaAtivaEntidade) 
                                                       VALUES (@CodigoPessoa,          {10},        '{16}',           '{15}',          '{17}', 'S')

                            SELECT @CodigoPessoa AS CodigoPessoa

                          END
                        ", bancodb, Ownerdb
                         , nomePessoa.Replace("'", "''")
                         , tipoPessoa
                         , numeroCNPJCPF
                         , enderecoPessoa.Replace("'", "''")
                         , telefonePessoa
                         , nomeContato.Replace("'", "''")
                         , informacaoContato.Replace("'", "''")
                         , codigoMunicipio == -1 ? "NULL" : codigoMunicipio.ToString()
                         , codigoEntidade
                         , codigoRamoAtividade
                         , email
                         , comentarios.Replace("'", "''")
                         , nomeFantasia.Replace("'", "''")
                         , indicaFornecedor
                         , indicaCliente
                         , indicaParticipe);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoPessoa"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaFornecedor(int codigoPessoa, string nomePessoa, string nomeFantasia, string tipoPessoa, string numeroCNPJCPF, string codigoRamoAtividade, string enderecoPessoa, string telefonePessoa
        , string nomeContato, string email, string informacaoContato, string comentarios, int codigoMunicipio, string indicaCliente, string indicaFornecedor, string indicaParticipe)
    {
        bool retorno = false;
        try
        {
            if (numeroCNPJCPF == "")
                numeroCNPJCPF = "NULL";
            else
                numeroCNPJCPF = "'" + numeroCNPJCPF + "'";

            if (telefonePessoa.Replace("(", "").Replace(")", "").Replace("-", "").Trim() == "")
                telefonePessoa = "NULL";
            else
                telefonePessoa = "'" + telefonePessoa + "'";

            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.Pessoa SET NomePessoa = '{2}'
                                                , TipoPessoa = '{3}'
                                                , NumeroCNPJCPF = {4}
                                                , EnderecoPessoa = '{5}'
                                                , TelefonePessoa = {6}
                                                , NomeContato = '{7}'
                                                , InformacaoContato = '{8}'
                                                , CodigoMunicipioEnderecoPessoa = {9}
                                                , CodigoRamoAtividade = {11}
                                                , Email = {12}
                                                , Comentarios = '{13}'
                                                , NomeFantasia = '{14}'
                         WHERE CodigoPessoa = {10} 

                         UPDATE {0}.{1}.PessoaEntidade  
                                 SET  IndicaCliente = '{16}'
                                      , IndicaFornecedor = '{15}' 
                                      , IndicaParticipe = '{17}'
                                 WHERE CodigoPessoa = {10} 

                        ", bancodb, Ownerdb
                         , nomePessoa.Replace("'", "''")
                         , tipoPessoa
                         , numeroCNPJCPF
                         , enderecoPessoa.Replace("'", "''")
                         , telefonePessoa
                         , nomeContato.Replace("'", "''")
                         , informacaoContato.Replace("'", "''")
                         , codigoMunicipio == -1 ? "NULL" : codigoMunicipio.ToString()
                         , codigoPessoa
                         , codigoRamoAtividade
                         , email
                         , comentarios.Replace("'", "''")
                         , nomeFantasia.Replace("'", "''")
                         , indicaFornecedor
                         , indicaCliente
                         , indicaParticipe);

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

    public bool verificaPendenciasFornecedor(int codigoFornecedor, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT 1
              FROM {0}.{1}.Contrato c INNER join
                    {0}.{1}.Pessoa  pes on ( pes.CodigoPessoa = c.CodigoPessoaContratada )  INNER JOIN 
                    {0}.{1}.[PessoaEntidade] AS [pe] ON (
			        pe.[CodigoPessoa] = c.[CodigoPessoaContratada]
			        AND pe.codigoEntidade = c.codigoEntidade
                    --AND pe.IndicaFornecedor = 'S'
			        ) 
             WHERE c.CodigoPessoaContratada = {2}
               {3}",
            bancodb, Ownerdb, codigoFornecedor, where);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            return true;

        return false;
    }

    public bool excluiFornecedor(int codigoEntidade, int codigoPessoa)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                            UPDATE {0}.{1}.PessoaEntidade SET IndicaPessoaAtivaEntidade = 'N'
                             WHERE CodigoPessoa = {2}
                               AND CodigoEntidade = {3}
                        ", bancodb, Ownerdb
                         , codigoPessoa
                         , codigoEntidade);

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

    public bool excluiFornecedorABC(int codigoUsuario, int codigoPessoa, int codigoProjeto)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                            UPDATE {0}.{1}.SENAR_FornecedorABC SET CodigoUsuarioExclusao = {3}
                                ,DataExclusao = GetDate()
                             WHERE CodigoPessoaFornecedor = {2}
                               AND CodigoProjeto = {4}
                        ", bancodb, Ownerdb
                         , codigoPessoa
                         , codigoUsuario
                         , codigoProjeto);

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

    #region Municípios
    public DataSet getMunicipios(string where)
    {

        comandoSQL = string.Format(@"
            SELECT m.CodigoMunicipio, m.NomeMunicipio, m.SiglaUF
             FROM {0}.{1}.Municipio m
            WHERE 1=1 
              {2}
            ORDER BY m.SiglaUF, m.NomeMunicipio", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getMunicipiosObra(string where)
    {

        comandoSQL = string.Format(@"
            SELECT m.CodigoMunicipio, m.NomeMunicipio, m.SiglaUF, mo.SiglaMunicipio
             FROM {0}.{1}.Municipio m INNER JOIN
                  {0}.{1}.MunicipioObra mo ON mo.CodigoMunicipio = m.CodigoMunicipio
            WHERE 1=1 
              {2}
            ORDER BY m.SiglaUF, m.NomeMunicipio", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getStatusObra(string where)
    {

        comandoSQL = string.Format(@"
            SELECT CodigoStatus, DescricaoStatus 
              FROM Status s 
             WHERE s.CodigoStatus IN(SELECT p.CodigoStatusProjeto 
                                       FROM Projeto p INNER JOIN
                                            Obra o ON o.CodigoProjeto = p.CodigoProjeto)
              {2}
            ORDER BY DescricaoStatus", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiMunicipioObra(string nomeMunicipio, string siglaUF, string siglaMunicipio, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoMunicipio int


                            IF NOT EXISTS(SELECT 1 FROM {0}.{1}.Municipio WHERE NomeMunicipio = '{2}' AND SiglaUF = '{3}')
                              BEGIN
                                INSERT INTO {0}.{1}.Municipio(NomeMunicipio, SiglaUF) 
                                 VALUES ('{2}', '{3}')

                                SELECT @CodigoMunicipio = SCOPE_IDENTITY()

                              END
                            ELSE
                                SELECT @CodigoMunicipio = CodigoMunicipio FROM {0}.{1}.Municipio WHERE NomeMunicipio = '{2}' AND SiglaUF = '{3}'
                            
                            INSERT INTO {0}.{1}.MunicipioObra(CodigoMunicipio, SiglaMunicipio) 
                                 VALUES (@CodigoMunicipio, '{4}')

                            SELECT @CodigoMunicipio AS CodigoMunicipio

                          END
                        ", bancodb, Ownerdb
                         , nomeMunicipio.Replace("'", "''")
                         , siglaUF
                         , siglaMunicipio);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoMunicipio"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaMunicipioObra(int codigoMunicipio, string nomeMunicipio, string siglaUF, string siglaMunicipio)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoMunicipio int

                            SET @CodigoMunicipio = {2}
                            
                            UPDATE {0}.{1}.Municipio SET NomeMunicipio = '{3}'
                                                        ,SiglaUF = '{4}'
                             WHERE CodigoMunicipio = @CodigoMunicipio 

                            UPDATE {0}.{1}.MunicipioObra SET SiglaMunicipio = '{5}'
                             WHERE CodigoMunicipio = @CodigoMunicipio 
                        END
                        ", bancodb, Ownerdb
                         , codigoMunicipio
                         , nomeMunicipio.Replace("'", "''")
                         , siglaUF
                         , siglaMunicipio);

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

    public bool excluiMunicipioObra(int codigoMunicipio)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoMunicipio int

                            SET @CodigoMunicipio = {2}
                            
                            DELETE FROM {0}.{1}.MunicipioObra
                             WHERE CodigoMunicipio = @CodigoMunicipio                           
                        END
                        ", bancodb, Ownerdb
                         , codigoMunicipio);

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

    public bool verificaExclusaoMunicipio(int codigoMunicipio)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Pessoa p
                       WHERE p.CodigoMunicipioEnderecoPessoa = {2}
                        ", bancodb, Ownerdb
                         , codigoMunicipio);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool verificaExclusaoMunicipioObra(int codigoMunicipio)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Obra o
                       WHERE o.CodigoMunicipioObra = {2}
                        ", bancodb, Ownerdb
                         , codigoMunicipio);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool incluiMunicipio(string nomeMunicipio, string siglaUF, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoMunicipio int

                            INSERT INTO {0}.{1}.Municipio(NomeMunicipio, SiglaUF) 
                                 VALUES ('{2}', '{3}')

                            SELECT @CodigoMunicipio = SCOPE_IDENTITY()

                            SELECT @CodigoMunicipio AS CodigoMunicipio

                          END
                        ", bancodb, Ownerdb
                         , nomeMunicipio.Replace("'", "''")
                         , siglaUF);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoMunicipio"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaMunicipio(int codigoMunicipio, string nomeMunicipio, string siglaUF)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoMunicipio int

                            SET @CodigoMunicipio = {2}
                            
                            UPDATE {0}.{1}.Municipio SET NomeMunicipio = '{3}'
                                                        ,SiglaUF = '{4}'
                             WHERE CodigoMunicipio = @CodigoMunicipio 
                        END
                        ", bancodb, Ownerdb
                         , codigoMunicipio
                         , nomeMunicipio.Replace("'", "''")
                         , siglaUF);

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

    public bool excluiMunicipio(int codigoMunicipio)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoMunicipio int

                            SET @CodigoMunicipio = {2}
                            
                            DELETE FROM {0}.{1}.MunicipioObra
                             WHERE CodigoMunicipio = @CodigoMunicipio 

                            DELETE FROM {0}.{1}.Municipio
                             WHERE CodigoMunicipio = @CodigoMunicipio                             
                        END
                        ", bancodb, Ownerdb
                         , codigoMunicipio);

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

    #region Segmento de Obra

    public DataSet getTipoSegmentoObra(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT tso.CodigoSegmentoObra, tso.DescricaoSegmentoObra
              FROM {0}.{1}.TipoSegmentoObra tso
             WHERE tso.CodigoEntidade = {2}
              {3}
            ORDER BY tso.DescricaoSegmentoObra", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiTipoSegmentoObra(string descricaoSegmentoObra, int codigoEntidade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @Codigo int

                            INSERT INTO {0}.{1}.TipoSegmentoObra(DescricaoSegmentoObra, CodigoEntidade) 
                                 VALUES ('{2}', {3})

                            SELECT @Codigo = SCOPE_IDENTITY()

                            SELECT @Codigo AS Codigo

                          END
                        ", bancodb, Ownerdb
                         , descricaoSegmentoObra.Replace("'", "''")
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["Codigo"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoSegmentoObra(int codigoSegmentoObra, string descricaoSegmentoObra)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}
                            
                            UPDATE {0}.{1}.TipoSegmentoObra SET DescricaoSegmentoObra = '{3}'
                             WHERE CodigoSegmentoObra = @Codigo 
                        END
                        ", bancodb, Ownerdb
                         , codigoSegmentoObra
                         , descricaoSegmentoObra.Replace("'", "''"));

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

    public bool excluiTipoSegmentoObra(int codigoSegmentoObra)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}

                            DELETE FROM {0}.{1}.TipoSegmentoObra
                             WHERE CodigoSegmentoObra = @Codigo                             
                        END
                        ", bancodb, Ownerdb
                         , codigoSegmentoObra);

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

    public bool verificaExclusaoTipoSegmentoObra(int codigoSegmentoObra)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Obra o
                       WHERE o.CodigoSegmentoObra = {2}
                        ", bancodb, Ownerdb
                         , codigoSegmentoObra);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region Critério de reajuste do contrato
    public DataSet getTipoCriterioReajusteContrato(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT trc.CodigoCriterioReajusteContrato, trc.DescricaoCriterioReajusteContrato
              FROM {0}.{1}.TipoCriterioReajusteContrato trc
             WHERE trc.CodigoEntidade = {2}
              {3}
            ORDER BY trc.DescricaoCriterioReajusteContrato", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiTipoCriterioReajusteContrato(string criterioReajusteContrato, int codigoEntidade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoCriterioReajusteContrato int

                            INSERT INTO {0}.{1}.TipoCriterioReajusteContrato(DescricaoCriterioReajusteContrato, CodigoEntidade) 
                                 VALUES ('{2}', {3})

                            SELECT @CodigoCriterioReajusteContrato = SCOPE_IDENTITY()

                            SELECT @CodigoCriterioReajusteContrato AS CodigoCriterioReajusteContrato

                          END
                        ", bancodb, Ownerdb
                         , criterioReajusteContrato.Replace("'", "''")
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoCriterioReajusteContrato"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoCriterioReajusteContrato(int codigoCriterioReajusteContrato, string criterioReajusteContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCriterioReajusteContrato int

                            SET @CodigoCriterioReajusteContrato = {2}
                            
                            UPDATE {0}.{1}.TipoCriterioReajusteContrato SET DescricaoCriterioReajusteContrato = '{3}'
                             WHERE CodigoCriterioReajusteContrato = @CodigoCriterioReajusteContrato 
                        END
                        ", bancodb, Ownerdb
                         , codigoCriterioReajusteContrato
                         , criterioReajusteContrato.Replace("'", "''"));

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

    public bool excluiTipoCriterioReajusteContrato(int codigoCriterioReajusteContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCriterioReajusteContrato int

                            SET @CodigoCriterioReajusteContrato = {2}

                            DELETE FROM {0}.{1}.TipoCriterioReajusteContrato
                             WHERE CodigoCriterioReajusteContrato = @CodigoCriterioReajusteContrato                             
                        END
                        ", bancodb, Ownerdb
                         , codigoCriterioReajusteContrato);

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

    public bool verificaExclusaoTipoCriterioReajusteContrato(int codigoCriterioReajusteContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Contrato c
                       WHERE c.CodigoCriterioReajusteContrato = {2}
                        ", bancodb, Ownerdb
                         , codigoCriterioReajusteContrato);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    public DataSet getTipoServico(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT ts.CodigoTipoServico, ts.DescricaoTipoServico, ts.IndicaObra
              FROM {0}.{1}.TipoServico ts
             WHERE ts.CodigoEntidade = {2}
              {3}
            ORDER BY ts.DescricaoTipoServico", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    #region Origem da Contratação
    public DataSet getTipoOrigemContrato(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT too.CodigoOrigemObra, too.DescricaoOrigemObra
              FROM {0}.{1}.TipoOrigemObra too
             WHERE too.CodigoEntidade = {2}
              {3}
            ORDER BY too.DescricaoOrigemObra", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiOrigemContratacao(string descricaoOrigemContratacao, int codigoEntidade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @Codigo int

                            INSERT INTO {0}.{1}.TipoOrigemObra(DescricaoOrigemObra, CodigoEntidade) 
                                 VALUES ('{2}', {3})

                            SELECT @Codigo = SCOPE_IDENTITY()

                            SELECT @Codigo AS Codigo

                          END
                        ", bancodb, Ownerdb
                         , descricaoOrigemContratacao.Replace("'", "''")
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["Codigo"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaOrigemContratacao(int codigoOrigemContratacao, string descricaoOrigemContratacao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}
                            
                            UPDATE {0}.{1}.TipoOrigemObra SET DescricaoOrigemObra = '{3}'
                             WHERE CodigoOrigemObra = @Codigo 
                        END
                        ", bancodb, Ownerdb
                         , codigoOrigemContratacao
                         , descricaoOrigemContratacao.Replace("'", "''"));

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

    public bool excluiOrigemContratacao(int codigoOrigemContratacao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}

                            DELETE FROM {0}.{1}.TipoOrigemObra 
                             WHERE CodigoOrigemObra = @Codigo                           
                        END
                        ", bancodb, Ownerdb
                         , codigoOrigemContratacao);

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

    public bool verificaExclusaoOrigemContratacao(int codigoOrigemContratacao)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Obra o
                       WHERE o.CodigoOrigemObra = {2}
                        ", bancodb, Ownerdb
                         , codigoOrigemContratacao);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region Fonte de recurso financeiro
    public DataSet getFontesRecursosFinanceiros(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT frf.CodigoFonteRecursosFinanceiros, frf.NomeFonte
              FROM {0}.{1}.FonteRecursosFinanceiros frf
             WHERE frf.CodigoEntidade = {2}
               AND frf.DataExclusao IS NULL
              {3}
            ORDER BY frf.NomeFonte", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiFonteRecursoFinanceiro(string descricaoFonteRecursoFinanceiro, int codigoEntidade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @Codigo int

                            INSERT INTO {0}.{1}.FonteRecursosFinanceiros(NomeFonte, CodigoEntidade) 
                                 VALUES ('{2}', {3})

                            SELECT @Codigo = SCOPE_IDENTITY()

                            SELECT @Codigo AS Codigo

                          END
                        ", bancodb, Ownerdb
                         , descricaoFonteRecursoFinanceiro.Replace("'", "''")
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["Codigo"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaFonteRecursoFinanceiro(int codigoFonteRecursoFinanceiro, string descricaoFonteRecursoFinanceiro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}
                            
                            UPDATE {0}.{1}.FonteRecursosFinanceiros SET NomeFonte = '{3}'
                             WHERE CodigoFonteRecursosFinanceiros = @Codigo 
                        END
                        ", bancodb, Ownerdb
                         , codigoFonteRecursoFinanceiro
                         , descricaoFonteRecursoFinanceiro.Replace("'", "''"));

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

    public bool excluiFonteRecursoFinanceiro(int codigoFonteRecursoFinanceiro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @Codigo int

                            SET @Codigo = {2}

                            UPDATE {0}.{1}.FonteRecursosFinanceiros SET DataExclusao = GetDate()
                             WHERE CodigoFonteRecursosFinanceiros = @Codigo                           
                        END
                        ", bancodb, Ownerdb
                         , codigoFonteRecursoFinanceiro);

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

    public bool verificaExclusaoFonteRecursoFinanceiro(int codigoFonteRecursoFinanceiro)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Contrato o
                       WHERE o.CodigoFonteRecursosFinanceiros = {2}
                        ", bancodb, Ownerdb
                         , codigoFonteRecursoFinanceiro);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region Critério de medição do contrato
    public DataSet getTipoCriterioMedicaoContrato(int codigoEntidade, string where)
    {

        comandoSQL = string.Format(@"
            SELECT tcm.CodigoCriterioMedicaoContrato, tcm.DescricaoCriterioMedicaoContrato
              FROM {0}.{1}.TipoCriterioMedicaoContrato tcm
             WHERE tcm.CodigoEntidade = {2}
              {3}
            ORDER BY tcm.DescricaoCriterioMedicaoContrato", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool incluiTipoCriterioMedicaoContrato(string criterioMedicaoContrato, int codigoEntidade, ref int novoCodigo)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                            DECLARE @CodigoCriterioMedicaoContrato int

                            INSERT INTO {0}.{1}.TipoCriterioMedicaoContrato(DescricaoCriterioMedicaoContrato, CodigoEntidade) 
                                 VALUES ('{2}', {3})

                            SELECT @CodigoCriterioMedicaoContrato = SCOPE_IDENTITY()

                            SELECT @CodigoCriterioMedicaoContrato AS CodigoCriterioMedicaoContrato

                          END
                        ", bancodb, Ownerdb
                         , criterioMedicaoContrato.Replace("'", "''")
                         , codigoEntidade);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigo = int.Parse(ds.Tables[0].Rows[0]["CodigoCriterioMedicaoContrato"].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaTipoCriterioMedicaoContrato(int codigoCriterioMedicaoContrato, string criterioMedicaoContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCriterioMedicaoContrato int

                            SET @CodigoCriterioMedicaoContrato = {2}
                            
                            UPDATE {0}.{1}.TipoCriterioMedicaoContrato SET DescricaoCriterioMedicaoContrato = '{3}'
                             WHERE CodigoCriterioMedicaoContrato = @CodigoCriterioMedicaoContrato 
                        END
                        ", bancodb, Ownerdb
                         , codigoCriterioMedicaoContrato
                         , criterioMedicaoContrato.Replace("'", "''"));

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

    public bool excluiTipoCriterioMedicaoContrato(int codigoCriterioMedicaoContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      BEGIN
                            DECLARE @CodigoCriterioMedicaoContrato int

                            SET @CodigoCriterioMedicaoContrato = {2}

                            DELETE FROM {0}.{1}.TipoCriterioMedicaoContrato
                             WHERE CodigoCriterioMedicaoContrato = @CodigoCriterioMedicaoContrato                             
                        END
                        ", bancodb, Ownerdb
                         , codigoCriterioMedicaoContrato);

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

    public bool verificaExclusaoTipoCriterioMedicaoContrato(int codigoCriterioMedicaoContrato)
    {
        bool retorno = false;
        try
        {
            comandoSQL = string.Format(@"
                      SELECT 1 
                        FROM {0}.{1}.Contrato c
                       WHERE c.CodigoCriterioMedicaoContrato = {2}
                        ", bancodb, Ownerdb
                         , codigoCriterioMedicaoContrato);


            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                retorno = false;
            else
                retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }
    #endregion

    #region Menu Tipo Projeto
    public DataSet getTipoProjetoPorIniciais()
    {

        comandoSQL = string.Format(@"
            SELECT CodigoTipoProjeto
                   ,TipoProjeto
                   ,IndicaTipoProjeto 
              FROM {0}.{1}.TipoProjeto WHERE CodigoTipoAssociacao IS NOT NULL", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }


    public DataSet getItemMenuObjeto(int codigoTipoProjetoSelecionado, int codigoEntidade, string iniciaisTipoAssociacao)
    {
        comandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoTipoAssociacao int

            SELECT @CodigoTipoAssociacao = CodigoTipoAssociacao 
              FROM {0}.{1}.TipoProjeto
             WHERE CodigoTipoProjeto = {2} 

           SELECT CodigoItemMenuObjeto,
                  DescricaoItemMenuObjeto
             FROM {0}.{1}.ItemMenuObjeto 
            WHERE CodigoTipoAssociacao = @CodigoTipoAssociacao
              AND CodigoItemMenuObjeto 
           NOT IN (SELECT mtp.CodigoItemMenu
				     FROM {0}.{1}.MenuTipoProjeto mtp 
               INNER JOIN {0}.{1}.GrupoMenuTipoProjeto gmtp 
                       ON (gmtp.CodigoGrupoMenuTipoProjeto = mtp.CodigoGrupoMenu
				       AND gmtp.CodigoTipoProjeto = {2})
                       AND gmtp.CodigoEntidade = {4})
          ORDER BY DescricaoItemMenuObjeto
        END
         ", bancodb, Ownerdb, codigoTipoProjetoSelecionado, iniciaisTipoAssociacao, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getMenuTipoProjeto(int codigoGrupo)
    {
        comandoSQL = string.Format(@"
         SELECT mtp.CodigoGrupoMenu
               ,mtp.CodigoItemMenu
               ,mtp.DescricaoOpcaoMenu
               ,mtp.SequenciaItemMenuGrupo
               ,imo.DescricaoItemMenuObjeto
          FROM {0}.{1}.MenuTipoProjeto mtp INNER JOIN 
               {0}.{1}.ItemMenuObjeto imo ON imo.CodigoItemMenuObjeto = mtp.CodigoItemMenu
        WHERE mtp.CodigoGrupoMenu = {2} 
     ORDER BY mtp.SequenciaItemMenuGrupo", bancodb, Ownerdb, codigoGrupo);
        return getDataSet(comandoSQL);
    }

    public bool atualizaMenuTipoProjeto(int codigoGrupoMenu, int codigoItemMenu, string descricaoOpcaoMenu, int ordem, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
           UPDATE {0}.{1}.[MenuTipoProjeto]
              SET [DescricaoOpcaoMenu] = '{2}'
                 ,[SequenciaItemMenuGrupo] = {3}
            WHERE CodigoGrupoMenu = {4}
                AND CodigoItemMenu = {5}", bancodb, Ownerdb, descricaoOpcaoMenu, ordem, codigoGrupoMenu, codigoItemMenu);
            return execSQL(comandoSQL, ref registrosAfetados);

        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool atualizaGrupoMenuTipoProjeto(string descricaoGrupoMenuTipoProjeto, int sequenciaGrupoMenuTipoProjeto, string indicaGrupoVisivel, int codigoMenuTipoProjeto, string mensagemErro)
    {
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
           BEGIN           

            UPDATE {0}.{1}.GrupoMenuTipoProjeto
               SET DescricaoGrupoMenuTipoProjeto = '{2}'
                   ,SequenciaGrupoMenuTipoProjeto = {3}
                   ,IndicaGrupoVisivel = '{4}'
             WHERE CodigoGrupoMenuTipoProjeto = {5}

           END"
                , bancodb, Ownerdb, descricaoGrupoMenuTipoProjeto, sequenciaGrupoMenuTipoProjeto, indicaGrupoVisivel, codigoMenuTipoProjeto);
            return execSQL(comandoSQL, ref registrosAfetados);

        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool verificaInclusaoGrupoMenuTipoProjeto(int codigoTipoProjetoEscolhido, string descricaoGrupoMenuTipoProjeto, int codigoEntidade)
    {
        comandoSQL = string.Format(@"
        SELECT 1
         FROM {0}.{1}.[GrupoMenuTipoProjeto]
        WHERE DescricaoGrupoMenuTipoProjeto like '{2}'
          AND CodigoTipoProjeto = {3}
          AND CodigoEntidade = {4}", bancodb, Ownerdb, descricaoGrupoMenuTipoProjeto, codigoTipoProjetoEscolhido, codigoEntidade);
        DataSet dsRegraMenu = getDataSet(comandoSQL);
        if (dsRegraMenu.Tables[0].Rows.Count > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool incluiGrupoMenuTipoProjeto(string descricaoGrupoMenuTipoProjeto, int codigoEntidade, int CodigoTipoProjeto, int sequenciaGrupoMenuTipoProjeto, char indicaGrupoVisivel, string mensagemErro)
    {
        int registrosAfetados = 0;
        try
        {
            string trataIniciaisGrupoMenu = "null";
            comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.GrupoMenuTipoProjeto(DescricaoGrupoMenuTipoProjeto, CodigoEntidade, CodigoTipoProjeto, SequenciaGrupoMenuTipoProjeto, IniciaisGrupoMenu, IndicaGrupoVisivel)
                     VALUES('{2}', {3}, {4}, {5}, {6}, '{7}')"
                , bancodb, Ownerdb, descricaoGrupoMenuTipoProjeto, codigoEntidade, CodigoTipoProjeto, sequenciaGrupoMenuTipoProjeto, trataIniciaisGrupoMenu, indicaGrupoVisivel);



            return execSQL(comandoSQL, ref registrosAfetados);

        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool incluiMenuTipoProjeto(int codigoGrupoMenu, int codigoItemMenu, string descricaoOpcaoMenu, int sequencia, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
                INSERT INTO {0}.{1}.MenuTipoProjeto(CodigoGrupoMenu,CodigoItemMenu,DescricaoOpcaoMenu ,SequenciaItemMenuGrupo)
                     VALUES({2}, {3}, '{4}', {5})"
                , bancodb, Ownerdb, codigoGrupoMenu, codigoItemMenu, descricaoOpcaoMenu, sequencia);
            return execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public bool excluiMenuTipoProjeto(int codigoItemMenu, int codigoGrupoMenu, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        try
        {
            comandoSQL = string.Format(@"
            DELETE FROM {0}.{1}.[MenuTipoProjeto]
             WHERE CodigoGrupoMenu = {2} 
               AND CodigoItemMenu = {3}", bancodb, Ownerdb, codigoGrupoMenu, codigoItemMenu);
            return execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            return false;
        }
    }

    public DataSet getGrupoMenuTipoObjeto(int codigoEntidade, int codigoTipoProjeto, string where)
    {
        comandoSQL = string.Format(@"
            SELECT CodigoGrupoMenuTipoProjeto
                   ,DescricaoGrupoMenuTipoProjeto
                   ,CodigoEntidade
                   ,CodigoTipoProjeto
                   ,SequenciaGrupoMenuTipoProjeto
                   ,IniciaisGrupoMenu
                   ,IndicaGrupoVisivel
              FROM {0}.{1}.GrupoMenuTipoProjeto
             WHERE CodigoEntidade = {2} 
               AND CodigoTipoProjeto = {3} 
          ORDER BY IndicaGrupoVisivel desc, SequenciaGrupoMenuTipoProjeto, DescricaoGrupoMenuTipoProjeto
", bancodb, Ownerdb, codigoEntidade, codigoTipoProjeto, where);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region Conta Desembolso Financeiro
    public bool incluiContaDesembolsoFinanceiro(string descricaoConta, char IndicaInvestimentoFinanciavel, string grupoConta, int codigoEntidade, int codigoCategoria, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;

        try
        {
            string comandoSQL = string.Format(@"
        INSERT INTO {0}.{1}.uhe_ContaDesembolsoFinanceiro
           (DescricaoConta, IndicaInvestimentoFinanciavel, GrupoConta
           ,CodigoEntidade, CodigoCategoria)
     VALUES('{2}'         ,'{3}'                         ,'{4}'
             ,{5}          ,{6})", getDbName(), getDbOwner(),
                                descricaoConta, IndicaInvestimentoFinanciavel, grupoConta,
                                codigoEntidade, codigoCategoria);
            execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaContaDesembolsoFinanceiro(string descricaoConta, char indicaInvestimentoFinanciavel, string grupoConta, int codigoEntidade, int codigoCategoria, int codigoConta, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = true;
        try
        {
            string comandoSQL = string.Format(@"
        UPDATE {0}.{1}.uhe_ContaDesembolsoFinanceiro
        SET DescricaoConta = '{2}'
           ,IndicaInvestimentoFinanciavel = '{3}'
           ,GrupoConta = '{4}'
           ,CodigoEntidade = {5}
           ,CodigoCategoria = {6}
       WHERE CodigoConta = {7}", getDbName(), getDbOwner(), descricaoConta, indicaInvestimentoFinanciavel, grupoConta, codigoEntidade, codigoCategoria, codigoConta);
            retorno = execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiContaDesembolsoFinanceiro(int codigoContaDesembolso, ref string mensagemErro)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
        DELETE FROM {0}.{1}.uhe_ContaDesembolsoFinanceiro
        WHERE CodigoConta = {2}", getDbName(), getDbOwner(), codigoContaDesembolso);
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

    #region _Portfolios - frameProposta_AdicionarUnidade

    public DataSet getDadosUnidadeProjeto(int idProjeto)
    {
        string comandoSQL = string.Format(@"
                select p.CodigoProjeto
			          ,p.NomeProjeto
	                  ,p.CodigoCategoria
                      ,ca.DescricaoCategoria
	                  ,p.CodigoUnidadeNegocio
	                  ,un.NomeUnidadeNegocio
	                  ,p.CodigoGerenteProjeto
	                  ,us.NomeUsuario
	                  ,p.CodigoMSProject
	                  ,p.DataUltimaAlteracao
                      ,Convert(Varchar, p.InicioProposta,103) as InicioProposta
                      ,Convert(Varchar, p.TerminoProposta,103) as TerminoProposta
                from {0}.{1}.Projeto p
                inner join Categoria ca on p.CodigoCategoria = ca.CodigoCategoria AND ca.DataExclusao IS NULL
                left join {0}.{1}.UnidadeNegocio un on p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio
                left join {0}.{1}.Usuario us        on p.CodigoGerenteProjeto = us.CodigoUsuario
                where p.CodigoProjeto = {2}", bancodb, Ownerdb, idProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getUnidadeNegocio(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT 
                         un.CodigoUnidadeNegocio
                        ,un.NomeUnidadeNegocio
                        ,un.NomeUnidadeNegocio + ' (' + un.SiglaUnidadeNegocio + ')' AS nomeUnidade
                        ,un.SiglaUnidadeNegocio
                FROM {0}.{1}.UnidadeNegocio un 
                WHERE 1=1
                  {2}
                ORDER BY un.NomeUnidadeNegocio
        ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUnidadeNegocioComProjetosAssociados(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT CodigoUnidadeNegocio, NomeUnidadeNegocio 
                  FROM UnidadeNegocio un
                 WHERE IndicaUnidadeNegocioAtiva = 'S'
                   AND CodigoEntidade = {2}
                   AND DataExclusao IS NULL
                   AND CodigoUnidadeNegocio <> CodigoEntidade
                   AND EXISTS(SELECT 1 
								FROM Projeto p 
							 WHERE p.DataExclusao IS NULL
								 AND p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio)
                   {3}
                ORDER BY un.NomeUnidadeNegocio
        ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUsuarioUnidadeNegocioAtivo(string where)
    {
        string comandoSQL = string.Format(@"
                select * from (
                SELECT  -1 CodigoUsuario
                      , '' NomeUsuario
                      , '' EMail
                      , '' CodigoUnidadeNegocio
                      , 'S' AS IndicaUsuarioAtivoUnidadeNegocio
                      , '' AS StatusUsuario
                union
                SELECT  us.CodigoUsuario
                      , us.NomeUsuario
                      , us.EMail
                      , un.CodigoUnidadeNegocio
                      ,uun.IndicaUsuarioAtivoUnidadeNegocio
                      ,CASE WHEN uun.IndicaUsuarioAtivoUnidadeNegocio = 'S' THEN '' ELSE 'INATIVO' END AS StatusUsuario
                FROM {0}.{1}.Usuario us
                INNER JOIN {0}.{1}.UsuarioUnidadeNegocio AS uun ON us.CodigoUsuario = uun.CodigoUsuario
                INNER JOIN {0}.{1}.UnidadeNegocio        AS un  ON un.CodigoUnidadeNegocio = uun.CodigoUnidadeNegocio

                WHERE us.DataExclusao IS NULL
                  AND un.DataExclusao IS NULL
                    {2}
                ) x
                ORDER BY x.IndicaUsuarioAtivoUnidadeNegocio DESC, x.NomeUsuario
                ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUsuariosAtivosEntidade(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT distinct us.CodigoUsuario, us.NomeUsuario, us.NomeUsuario + ' (' + us.EMail + ')' AS UsuarioMaisEmail, us.EMail
                  FROM {0}.{1}.Usuario us
                 INNER JOIN {0}.{1}.UsuarioUnidadeNegocio uun on us.CodigoUsuario = uun.CodigoUsuario
                 WHERE uun.IndicaUsuarioAtivoUnidadeNegocio = 'S'
                   AND uun.CodigoUnidadeNegocio = {2}
                   AND us.DataExclusao IS NULL
                   {3}
                 ORDER BY us.NomeUsuario
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMSProjet(int idProjeto)
    {
        string comandoSQL = string.Format(@"
                select lower(vi.CodigoMSProject) as CodigoMSProject , vi.NomeProjeto
                from {0}.{1}.vi_Projeto vi
                where vi.CodigoProjeto IS NULL 
		           or vi.CodigoProjeto= {2}
                ORDER BY vi.NomeProjeto", bancodb, Ownerdb, idProjeto);
        return getDataSet(comandoSQL);
    }

    public bool incluiDadosProjeto(int codigoUsuarioInclusao, int codigoEntidade, string nomeProjeto, string codigoCategoria, string codigoUnidadeNegocio, string CodigoGerenteProjeto,
                                            string codigoMSprojet, string codigoReservado, string descricaoProposta, string IndicaRecursosAtualizamTarefas,
                                            string IndicaTipoAtualizacaoTarefas, string IndicaAprovadorTarefas, int[] codigosRelatorios,
                                            string tipoProjeto, string unidadeAtendimento, string codigoCarteiraPrincipal, out int codigoProjetoIncluido, out string erro)
    {
        codigoProjetoIncluido = -1;
        erro = "";
        string comandoSQLRelatorios = "";
        string comandoSQLCarteiraPrincipal = string.Format(@"
        DECLARE @CodigoCarteira as int
        SET @CodigoCarteira = {0}

        IF (@CodigoCarteira is not null)
        BEGIN

            INSERT INTO CarteiraProjeto(CodigoCarteira,   CodigoProjeto, IndicaCarteiraPrincipal)
                                 VALUES(@CodigoCarteira, @CodigoProjeto, 'S')

           UPDATE CarteiraProjeto 
                   SET IndicaCarteiraPrincipal = 'N'
            WHERE CodigoProjeto = @CodigoProjeto

           UPDATE CarteiraProjeto 
                   SET IndicaCarteiraPrincipal = 'S'
            WHERE CodigoProjeto = @CodigoProjeto and CodigoCarteira = @CodigoCarteira

         END", codigoCarteiraPrincipal);
        try
        {
            for (int i = 0; i < codigosRelatorios.Length; i++)
            {
                comandoSQLRelatorios += string.Format(@"               
                     INSERT INTO {0}.{1}.[ModeloStatusReportProjeto] 
		     	                    ( [CodigoModeloStatusReport], [CodigoProjeto], [IndicaModeloAtivoProjeto])
	                    VALUES ( {2}, @CodigoProjeto, 'S');
                  ", bancodb, Ownerdb, codigosRelatorios[i]);
            }

            comandoSQL = string.Format(@"
                            BEGIN     
                             DECLARE @CodigoProjeto INT
                  
                             INSERT INTO {0}.{1}.Projeto(NomeProjeto, DataInclusao, CodigoUsuarioInclusao, CodigoCategoria, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade,CodigoGerenteProjeto, CodigoUnidadeNegocio, CodigoMSProject, CodigoTipoProjeto, CodigoReservado, DescricaoProposta, IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas, CodigoUnidadeAtendimento) VALUES
                                                        (      '{8}',    GETDATE(),                   {2},             {3},                   3,            'N',            {4},                 {5},                  {6},           {7},                {15},             {9}, '{10}'           ,'{11}'                         , {12}                        ,{13}                   , {16})                  
                             SELECT @CodigoProjeto = scope_identity()    
                        
                             {14}

                             EXEC {0}.{1}.p_AtualizaStatusProjetos 'Sistema', @CodigoProjeto 
                             SELECT @CodigoProjeto
                             
                             {17}                            
                            
                             END
            ", bancodb, Ownerdb, codigoUsuarioInclusao, codigoCategoria, codigoEntidade, CodigoGerenteProjeto, codigoUnidadeNegocio, codigoMSprojet, nomeProjeto, codigoReservado, descricaoProposta, IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas, comandoSQLRelatorios, tipoProjeto, unidadeAtendimento, comandoSQLCarteiraPrincipal);

            DataSet ds1 = getDataSet(comandoSQL);
            if (DataSetOk(ds1) && DataTableOk(ds1.Tables[0]))
            {
                codigoProjetoIncluido = int.Parse(ds1.Tables[0].Rows[0][0].ToString());
            }
            erro = "";
            return true;
        }
        catch (Exception ex)
        {

            erro = ex.Message;
            return false;
        }
    }

    public bool atualizaDadosProjeto(int codigoProjeto, int codigoUsuarioAlteracao, string nomeProjeto, string codigoCategoria, string codigoUnidadeNegocio, string CodigoGerenteProjeto,
                                            string codigoReservado, string descricaoProposta, string IndicaRecursosAtualizamTarefas, string IndicaTipoAtualizacaoTarefas, string IndicaAprovadorTarefas,
                                            string codigoMSProject, string tipoProjeto, string unidadeAtendimento, string codigoCarteiraPrincipal, out string msgErro)
    {
        int registrosAfetados = 0;

        string comandoSQLCarteiraPrincipal = string.Format(@"
        DECLARE @CodigoCarteira as int
        SET @CodigoCarteira = {0}
        IF(@CodigoCarteira is not null)
        BEGIN
            IF NOT EXISTS(SELECT 1 
                            FROM CarteiraProjeto 
                            WHERE CodigoCarteira = @CodigoCarteira
                            AND CodigoProjeto = @CodigoProjeto)
            BEGIN
                 INSERT INTO CarteiraProjeto(CodigoCarteira, CodigoProjeto, IndicaCarteiraPrincipal)
                                      VALUES(@CodigoCarteira, @CodigoProjeto, 'S')

                  UPDATE CarteiraProjeto 
                     SET IndicaCarteiraPrincipal = 'N'
                   WHERE CodigoProjeto = @CodigoProjeto

                  UPDATE CarteiraProjeto 
                     SET IndicaCarteiraPrincipal = 'S'
                   WHERE CodigoProjeto = @CodigoProjeto and CodigoCarteira = @CodigoCarteira
            

            END
            ELSE
            BEGIN
                  UPDATE CarteiraProjeto 
                    SET IndicaCarteiraPrincipal = 'N'
                  WHERE CodigoProjeto = @CodigoProjeto

                  UPDATE CarteiraProjeto 
                    SET IndicaCarteiraPrincipal = 'S'
                  WHERE CodigoCarteira = @CodigoCarteira  and CodigoProjeto = @CodigoProjeto
            END
         END
      ELSE
      BEGIN
        UPDATE CarteiraProjeto 
        SET IndicaCarteiraPrincipal = 'N'
        WHERE CodigoProjeto = @CodigoProjeto
      END", codigoCarteiraPrincipal);

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                               DECLARE @CodigoProjeto as Int
                                set @CodigoProjeto = {2}
                                UPDATE {0}.{1}.Projeto SET
                                        NomeProjeto = '{3}'
                                      , DataUltimaAlteracao = GetDate()
                                      , CodigoUsuarioUltimaAlteracao = {4}
                                      , CodigoCategoria = {5}
                                      , CodigoGerenteProjeto = {6}
                                      , CodigoUnidadeNegocio = {7}
                                      , CodigoReservado = {8}
                                      , DescricaoProposta = '{9}'
                                      , IndicaRecursosAtualizamTarefas = '{10}'
                                      , IndicaTipoAtualizacaoTarefas = {11}
                                      , IndicaAprovadorTarefas = {12}
                                      , codigoMSProject = {13}
                                      , CodigoTipoProjeto = {14}
                                      , CodigoUnidadeAtendimento = {15}
                                 WHERE  CodigoProjeto = @CodigoProjeto

                                UPDATE {0}.{1}.CronogramaProjeto SET NomeProjeto = '{3}'
                                 WHERE  CodigoProjeto = @CodigoProjeto  

                                UPDATE {0}.{1}.TarefaCronogramaProjeto SET NomeTarefa = '{3}'
                                 WHERE Nivel = 0
                                   AND DataExclusao IS NULL
                                   AND CodigoCronogramaProjeto IN(SELECT cp.CodigoCronogramaProjeto 
                                                                    FROM {0}.{1}.CronogramaProjeto cp
                                                                   WHERE  CodigoProjeto = @CodigoProjeto)

                                  {16}
                            
                        END        
            ", bancodb, Ownerdb, codigoProjeto, nomeProjeto, codigoUsuarioAlteracao, codigoCategoria, CodigoGerenteProjeto, codigoUnidadeNegocio, codigoReservado
             , descricaoProposta, IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas,
             codigoMSProject == "" ? "NULL" : ("'" + codigoMSProject + "'"), tipoProjeto, unidadeAtendimento, comandoSQLCarteiraPrincipal);

            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            return true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            return false;
        }
    }

    public string defineLabelUnidadeAtendimento()
    {
        DataSet dsParametro = this.getParametrosSistema("labelUnidadeAtendimento");
        string label = "Unidade de Atendimento";

        if ((this.DataSetOk(dsParametro)) && (this.DataTableOk(dsParametro.Tables[0])))
        {
            label = dsParametro.Tables[0].Rows[0]["labelUnidadeAtendimento"].ToString();
        }

        return label + ":";

    }

    public bool incluiDadosProjetoExtendido(int codigoUsuarioInclusao, int codigoEntidade, string nomeProjeto, string escopo, string resultados, string CodigoGerenteProjeto,
                                            string dataInicio, string dataTermino, string valorOrcamento, string moeda, out int codigoProjetoIncluido, out string erro)
    {
        erro = "";
        try
        {
            comandoSQL = string.Format(@"
                            BEGIN     
                             DECLARE @CodigoProjeto INT
                  
                             INSERT INTO {0}.{1}.Projeto(NomeProjeto, DataInclusao, CodigoUsuarioInclusao, DescricaoProposta, CodigoStatusProjeto, IndicaPrograma, Memo1, CodigoEntidade
                                                        ,CodigoGerenteProjeto, CodigoUnidadeNegocio, CodigoTipoProjeto, InicioProposta, TerminoProposta
                                                        ,IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas, Valor1, texto1) VALUES
                                                        ('{2}', GETDATE(), {3}, '{4}', 3, 'N', '{5}', {6}, {7}, {6}, 1, {8}, {9}, 'N', 'P', 'GP', {10}, '{11}')     
                             SELECT @CodigoProjeto = scope_identity()    
                                                     
                             EXEC {0}.{1}.p_AtualizaStatusProjetos 'Sistema', @CodigoProjeto 

                             SELECT @CodigoProjeto                            
                            END
            ", bancodb, Ownerdb, nomeProjeto, codigoUsuarioInclusao, escopo, resultados, codigoEntidade, CodigoGerenteProjeto, dataInicio, dataTermino, valorOrcamento.Replace(".", "").Replace(",", "."), moeda);

            codigoProjetoIncluido = -1;

            DataSet ds1 = getDataSet(comandoSQL);
            if (DataSetOk(ds1) && DataTableOk(ds1.Tables[0]))
            {
                codigoProjetoIncluido = int.Parse(ds1.Tables[0].Rows[0][0].ToString());
            }
            erro = "";

            return true;
        }
        catch (Exception ex)
        {
            codigoProjetoIncluido = -1;
            erro = ex.Message;
            return false;
        }
    }

    public bool atualizaDadosProjetoExtendido(int codigoProjeto, int codigoUsuarioAlteracao, string nomeProjeto, string escopo, string resultados, string CodigoGerenteProjeto,
                                            string dataInicio, string dataTermino, string valorOrcamento, string moeda, out string msgErro)
    {
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                             UPDATE {0}.{1}.Projeto SET
                                        NomeProjeto = '{3}'
                                      , DataUltimaAlteracao = GetDate()
                                      , CodigoUsuarioUltimaAlteracao = {4}
                                      , DescricaoProposta = '{5}'
                                      , Memo1 = '{6}'
                                      , CodigoGerenteProjeto = {7}
                                      , InicioProposta = {8}
                                      , TerminoProposta = {9}
                                      , Valor1 = {10}
                                      , texto1 = '{11}'
                                 WHERE  CodigoProjeto = {2}

                                UPDATE {0}.{1}.CronogramaProjeto SET NomeProjeto = '{3}'
                                 WHERE  CodigoProjeto = {2}  

                                UPDATE {0}.{1}.TarefaCronogramaProjeto SET NomeTarefa = '{3}'
                                 WHERE Nivel = 0
                                   AND DataExclusao IS NULL
                                   AND CodigoCronogramaProjeto IN(SELECT cp.CodigoCronogramaProjeto 
                                                                    FROM {0}.{1}.CronogramaProjeto cp
                                                                   WHERE  CodigoProjeto = {2})
                            
                        END        
            ", bancodb, Ownerdb, codigoProjeto, nomeProjeto, codigoUsuarioAlteracao, escopo, resultados, CodigoGerenteProjeto, dataInicio, dataTermino, valorOrcamento.Replace(".", "").Replace(",", "."), moeda);

            execSQL(comandoSQL, ref registrosAfetados);

            msgErro = "";

            return true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            return false;
        }
    }

    public bool incluiDadosProcesso(int codigoUsuarioInclusao, int codigoEntidade, string nomeProjeto, string codigoUnidadeNegocio, string CodigoGerenteProjeto,
                                            string codigoReservado, string descricaoProposta, string IndicaRecursosAtualizamTarefas, string IndicaTipoAtualizacaoTarefas, string IndicaAprovadorTarefas, out int codigoProjetoIncluido, out string erro)
    {
        codigoProjetoIncluido = -1;
        erro = "";
        try
        {
            comandoSQL = string.Format(@"
                            BEGIN     
                             DECLARE @CodigoProjeto INT,
                                     @CodigoStatus INT

                             SET @CodigoStatus = 21
                             
                             INSERT INTO {0}.{1}.Projeto(NomeProjeto, DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade,CodigoGerenteProjeto, CodigoUnidadeNegocio, CodigoTipoProjeto, CodigoReservado, DescricaoProposta, IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas) VALUES
                                                        (      '{6}',    GETDATE(),                   {2},       @CodigoStatus,            'N',            {3},                 {4},                  {5},           3,             {7},        '{8}'                    ,'{9}'                          , {10}                        ,{11})     
                             SELECT @CodigoProjeto = scope_identity()    
                             EXEC {0}.{1}.p_AtualizaStatusProjetos 'Sistema', @CodigoProjeto 
                             SELECT @CodigoProjeto                            
                            END
            ", bancodb, Ownerdb, codigoUsuarioInclusao, codigoEntidade, CodigoGerenteProjeto, codigoUnidadeNegocio, nomeProjeto, codigoReservado, descricaoProposta, IndicaRecursosAtualizamTarefas, IndicaTipoAtualizacaoTarefas, IndicaAprovadorTarefas);

            DataSet ds1 = getDataSet(comandoSQL);
            if (DataSetOk(ds1) && DataTableOk(ds1.Tables[0]))
            {
                codigoProjetoIncluido = int.Parse(ds1.Tables[0].Rows[0][0].ToString());
            }
            erro = "";
            return true;
        }
        catch (Exception ex)
        {

            erro = ex.Message;
            return false;
        }
    }



    public bool atualizaDadosUnidadeProjeto(string codigoProjeto,
                                            string codigoUnidadeNegocio, string CodigoGerenteProjeto,
                                            string codigoMSprojet, string inicioProposta,
                                            string terminoProposta)
    {
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN
                             UPDATE {0}.{1}.Projeto SET
                                        CodigoUnidadeNegocio = {3}
                                       ,CodigoGerenteProjeto = {4}
                                       ,CodigoMSProject      = '{5}'
                                       ,InicioProposta       = {6}
                                       ,TerminoProposta      = {7}
                                       ,DataUltimaAlteracao  = getdate()
                                 WHERE  CodigoProjeto   = {2}
                        END        
            ", bancodb, Ownerdb, codigoProjeto, codigoUnidadeNegocio, CodigoGerenteProjeto,
               codigoMSprojet, inicioProposta, terminoProposta);

            execSQL(comandoSQL, ref registrosAfetados);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region _Portfolios - Administracao - Configuracoes.aspx

    public bool incluiConfiguracoes(string codigoEntidade, string ano, string anoPeriodoEditavel, string tipoEdicao, string IndicaAnoAtivo, string IndicaMetaEditavel, string IndicaResultadoEditavel, ref string mensagemErro)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                        INSERT INTO {0}.{1}.PeriodoAnalisePortfolio (IndicaAnoPeriodoEditavel,CodigoEntidade, Ano, CodigoPeriodicidade,IndicaAnoAtivo,IndicaMetaEditavel,IndicaResultadoEditavel) 
                                                             VALUES ('{2}'                   ,           {3}, {4}, {5}                  ,'{6}'         ,'{7}'             ,'{8}')
                        ", bancodb, Ownerdb, anoPeriodoEditavel, codigoEntidade, ano, tipoEdicao, IndicaAnoAtivo, IndicaMetaEditavel, IndicaResultadoEditavel);

            DataSet ds = getDataSet(comandoSQL);
            //if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            //    identityCodigoProjeto = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message + Environment.NewLine + comandoSQL;
            if (mensagemErro.Contains("PRIMARY KEY") == true)
                mensagemErro = "O ano que se tenta incluir já existe, não podem haver anos repetidos.";
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaConfiguracoes(string codigoEntidade, string ano, string anoPeriodoEditavel, string tipoEdicao, string IndicaAnoAtivo, string IndicaMetaEditavel, string IndicaResultadoEditavel, ref string mensagemErro)
    {
        bool retorno = false;
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                        UPDATE {0}.{1}.PeriodoAnalisePortfolio SET
                            IndicaAnoPeriodoEditavel = '{2}', 
                            CodigoPeriodicidade = {3},
                            IndicaAnoAtivo  = '{6}',
                            IndicaMetaEditavel  = '{7}',
                            IndicaResultadoEditavel  = '{8}' 
                        WHERE Ano            = {4}
                          AND CodigoEntidade = {5}
                  ", bancodb, Ownerdb, anoPeriodoEditavel, tipoEdicao, ano, codigoEntidade, IndicaAnoAtivo, IndicaMetaEditavel, IndicaResultadoEditavel);

            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            mensagemErro = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region _Portfolios - Relatorios

    public DataSet getRelatorioProjetosPortfolio(int codigoportfolio, int codigoEntidade, int codigoCarteira, int codigoUsuario)
    {
        string comandoSQL = string.Format(@"
            SELECT p.NomeProjeto AS NomeProjeto,                  
                   un.SiglaUnidadeNegocio AS SiglaUnidade,
                   gu.NomeUsuario AS NomeGerenteUnidade,
                   u.NomeUsuario AS NomeGerenteProjeto,
                   c.DescricaoCategoria AS Categoria,
                   s.DescricaoStatus AS StatusProjeto,
                   f.DescricaoFaseGP AS FaseProjeto,
                   rp.CustoRealTotal AS CustoReal,
                   rp.CustoLBTotal AS CustoPrevistoTotal,
                   rp.CustoLBData AS CustoPrevistoData,
                   rp.CustoTendenciaTotal AS Custo,
                   rp.CustoRealHE AS CustoHE,
                   rp.TrabalhoRealTotal AS TRabalhoReal,
                   rp.TrabalhoLBTotal AS TrabalhoPrevistoTotal,
                   rp.TrabalhoLBData AS TrabalhoPrevistoData,
                   rp.TrabalhoTendenciaTotal AS Trabalho,
                   rp.TrabalhoRealHE AS TrabalhoHE,
                   rp.COTA AS ValorPlanejado,
                   rp.COTR AS ValorAgregado,
                   rp.ReceitaRealTotal AS ReceitaRealTotal,
                   rp.ReceitaLBTotal AS ReceitaPrevistaTotal,
                   rp.ReceitaLBData AS ReceitaPrevistaData,
                   rp.ReceitaTendenciaTotal AS Receita,
                   rp.ScoreCriterios AS EscoreCriterios,
                   rp.ScoreRiscos AS EscoreRiscos,
                   rp.TrabalhoTendenciaTotal - rp.TrabalhoLBTotal AS VariacaoTrabalho,
                   rp.CustoTendenciaTotal - rp.CustoLBTotal AS VariacaoCusto,
                   rp.ReceitaTendenciaTotal - rp.ReceitaLBTotal AS VariacaoReceita,
                   rp.EAC AS EstimativaCustoConcluir                                
              FROM {0}.{1}.ResumoProjeto AS rp INNER JOIN
                   {0}.{1}.Projeto AS p ON (rp.CodigoProjeto = p.CodigoProjeto) INNER JOIN
                   {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) INNER JOIN
                   {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria) INNER JOIN
                   {0}.{1}.PortfolioProjeto AS pp ON (pp.CodigoProjeto = p.CodigoProjeto) INNER JOIN
                   {0}.{1}.Portfolio AS Portf ON (Portf.CodigoPortfolio = pp.CodigoPortfolio
                                      AND Portf.CodigoPortfolio = {2}) INNER JOIN
                   {0}.{1}.Usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) INNER JOIN
                   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto) INNER JOIN
                   {0}.{1}.Status AS spp ON (spp.CodigoStatus = pp.CodigoStatusProjetoPortfolio
                                 AND spp.IndicaAcompanhamentoPortfolio = 'S') INNER JOIN
                   {0}.{1}.FaseGP AS f ON (f.CodigoFaseGP = s.CodigoFaseGP) INNER JOIN
                   {0}.{1}.Usuario AS gu ON (gu.CodigoUsuario = un.CodigoUsuarioGerente) INNER JOIN
                   {0}.{1}.f_GetProjetosUsuario({4},{3}, {5}) PU on (PU.CodigoProjeto = p.CodigoProjeto)               
             WHERE p.DataExclusao IS NULL and p.CodigoEntidade = {3}
        ", bancodb, Ownerdb, codigoportfolio, codigoEntidade, codigoUsuario, codigoCarteira);
        return getDataSet(comandoSQL);
    }

    public DataSet getUnidadePropostaProjeto(int codigoEntidade, int codigoCarteira, int codigoUsuario)
    {
        string comandoSQL = string.Format(@"
	        SELECT un.CodigoUnidadeNegocio, un.SiglaUnidadeNegocio 
	          FROM {0}.{1}.UnidadeNegocio un
	         WHERE un.CodigoEntidade = {2}
	           AND un.DataExclusao IS NULL
	           AND EXISTS(SELECT 1 
					        FROM {0}.{1}.Projeto p INNER JOIN
						         {0}.{1}.ProjetoObjetivoEstrategico poe ON poe.CodigoProjeto = p.CodigoProjeto INNER JOIN
						         {0}.{1}.f_GetProjetosUsuario({3}, {2},{4}) F on (f.codigoProjeto = p.CodigoProjeto)
				           WHERE p.CodigoUnidadeNegocio = un.CodigoUnidadeNegocio
					         AND p.CodigoEntidade = {2}
					         AND p.CodigoTipoProjeto IN(SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')
					         AND p.DataExclusao IS NULL)", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);
        return getDataSet(comandoSQL);
    }

    public DataSet getStatusRelPropostaProjetos()
    {
        string comandoSQL = string.Format("select CodigoStatus,DescricaoStatus from {0}.{1}.Status where IndicaListaPropostaProjeto = 'S'", bancodb, Ownerdb);
        return getDataSet(comandoSQL);
    }

    public DataSet getRelProjetosPropostas(int codigoEntidadeUsuarioResponsavel, int codigoUsuarioLogado, int codigoCarteira, string where)
    {
        string comandoSQL = string.Format(@"
     	SELECT p.NomeProjeto AS Titulo,
		   p.CodigoProjeto,
		   un.NomeUnidadeNegocio,
		   c.DescricaoCategoria,
		   s.DescricaoStatus,
		   p.DescricaoProposta AS Descricao,
           'Objetivos Estratégicos' as tituloOE,
		   oe.DescricaoObjetoEstrategia
	  FROM {0}.{1}.ResumoProjeto AS rp INNER JOIN
		   {0}.{1}.Projeto AS p ON (p.CodigoProjeto = rp.CodigoProjeto
						AND p.CodigoEntidade = {2}
						AND p.DataExclusao IS NULL
						AND p.CodigoTipoProjeto IN (SELECT tp.CodigoTipoProjeto FROM TipoProjeto tp WHERE tp.IndicaTipoProjeto = 'PRJ' OR tp.IndicaTipoProjeto = 'PRG')) LEFT JOIN
		   {0}.{1}.Categoria AS c ON (c.CodigoCategoria = p.CodigoCategoria
								  AND c.DataExclusao IS NULL) LEFT JOIN
		   {0}.{1}.UnidadeNegocio AS un ON (un.CodigoUnidadeNegocio = p.CodigoUnidadeNegocio) LEFT JOIN
		   {0}.{1}.Usuario AS g ON (g.CodigoUsuario = p.CodigoGerenteProjeto) INNER JOIN
		   {0}.{1}.Status AS s ON (s.CodigoStatus = p.CodigoStatusProjeto
                               and s.IndicaListaPropostaProjeto = 'S') INNER JOIN
		   {0}.{1}.f_GetProjetosUsuario({3}, {2}, {4}) F on (f.codigoProjeto = p.CodigoProjeto) INNER JOIN
		   {0}.{1}.ProjetoObjetivoEstrategico poe ON poe.CodigoProjeto = p.CodigoProjeto INNER JOIN
		   {0}.{1}.ObjetoEstrategia oe ON oe.CodigoObjetoEstrategia = poe.CodigoObjetivoEstrategico
	 WHERE p.CodigoEntidade = {2}
	   {5}", bancodb, Ownerdb, codigoEntidadeUsuarioResponsavel, codigoUsuarioLogado, codigoCarteira, where);
        return getDataSet(comandoSQL);
    }

    #endregion

    #region _Portfolios - Administracao - amd_CadastroDePerfis

    public DataSet getPerfisWorkFlow(string codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT CodigoPerfilWf
                    ,  NomePerfilWf AS perfil
                    ,  TipoPerfilWf AS tipo
                    ,  CASE TipoPerfilWf
                          WHEN 'PE' THEN '" + Resources.traducao.adm_CadastroDePerfis_pessoas_espec_ficas + @"' 
                          WHEN 'RF' THEN 'Recursos de Fluxos' 
                         ELSE '" + Resources.traducao.adm_CadastroDePerfis_recurso_de_projeto + @"' END AS indTipoGrupo
                FROM {0}.{1}.PerfisWf
                WHERE 1=1
                  AND CodigoEntidade = {2}
                  {3}
                ORDER BY NomePerfilWf
                ", bancodb, Ownerdb, codigoEntidade, where);

        return getDataSet(comandoSQL);
    }

    public bool habilitarExcluirPerfisWf(string codigoPerfis)
    {
        try
        {
            bool bPermissao = false;

            string comandoSQL = string.Format(@" SELECT {0}.{1}.f_wf_VerificaPermissaoExclusaoPerfilWf({2}) AS Permissao;", bancodb, Ownerdb, codigoPerfis);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds))
                if (ds.Tables[0].Rows.Count > 0)
                    bPermissao = (bool)ds.Tables[0].Rows[0]["Permissao"];

            return bPermissao;
        }
        catch (Exception ex)
        {
            return false;
            throw new Exception(getErroExcluiRegistro(ex.Message));
        }
    }

    #endregion

    #region administracao_adm_CadastroPerfis

    public DataSet getModeloFormulario(string where)
    {
        string comandoSQL = string.Format(
            @" SELECT CodigoModeloFormulario
                     ,NomeFormulario
               FROM {0}.{1}.ModeloFormulario WHERE 1=1 {2}",
               bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    public DataSet getCampoModeloFormulario(string where, int codigoModeloFormulario)
    {
        string comandoSQL = string.Format(
            @" SELECT CodigoCampo
                      ,NomeCampo
               FROM {0}.{1}.CampoModeloFormulario 
               WHERE 1=1 {2}
               AND CodigoModeloFormulario = {3}",
               bancodb, Ownerdb, where, codigoModeloFormulario);

        return getDataSet(comandoSQL);
    }

    public bool excluirPerfilNotificacaoWorkflow(string nomeGrupo, string tipoAcao)
    {
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
            DECLARE @retorno varchar(5)
			SET     @retorno = 'true'

            BEGIN TRY
                BEGIN TRAN
                UPDATE {0}.{1}.[DetalhesTGPWfPessoasEspecificas] SET [StatusRegistro] = 'D', [DataDesativacaoRegistro] = GETDATE() 
                     WHERE [NomeTipoGrupoPessoasWf] = '{2}'
                UPDATE {0}.{1}.[TiposGruposPessoasWf] SET [StatusGrupo] = 'D', [DataDesativacaoGrupo] = GETDATE() 
                     WHERE NomeTipoGrupoPessoasWf = '{2}'
                COMMIT
            END TRY
            BEGIN CATCH
                ROLLBACK
                set @retorno = 'false'
            END CATCH
            select @retorno"
            , bancodb, Ownerdb, nomeGrupo);

            DataSet ds = getDataSet(comandoSQL);
            if (ds.Tables[0].Rows[0][0].ToString() == "true")
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        catch (Exception ex)
        {
            throw new Exception(getErroExcluiRegistro(ex.Message));
        }
    }
    #endregion
}
}