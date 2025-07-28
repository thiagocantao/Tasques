using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

/// <summary>
/// Summary description for myDados
/// </summary>


namespace BriskPtf
{
public partial class dados
{
    #region administracao - recursos corporativos e grupos

    public DataSet getGruposRecursos(int codigoEntidade, string where)
    {
        comandoSQL = string.Format(@"
                    SELECT  gr.CodigoGrupoRecurso AS CodigoGrupoRecurso, 
                            gr.DescricaoGrupo AS DescricaoGrupo, 
                            gr.DetalheGrupo AS DetalheGrupo,
                            gr.GrupoRecursoSuperior AS GrupoRecursoSuperior, 
                            gr.NivelGrupo AS NivelGrupo,
                            gr.CodigoEntidade AS CodigoEntidade,
                            gr.CodigoTipoRecurso AS CodigoTipoRecurso,
                            gr.CustoHora AS CustoHora,
                            gr.CustoUso AS CustoUso, 
                            gr.CustoHoraExtra AS CustoHoraExtra,
                            gr.UnidadeMedida AS UnidadeMedida,
                            grs.DescricaoGrupo AS DescricaoGrupoSuperior,
                            tr.DescricaoTipoRecurso
                    FROM    {0}.{1}.GrupoRecurso AS gr LEFT JOIN
                            {0}.{1}.GrupoRecurso AS grs ON (grs.CodigoGrupoRecurso = gr.GrupoRecursoSuperior) INNER JOIN
                            {0}.{1}.TipoRecurso  AS tr  ON (tr.CodigoTipoRecurso = gr.CodigoTipoRecurso) 

                    WHERE   gr.CodigoEntidade = {2}
                       {3}
                    ORDER BY gr.DescricaoGrupo
                    ", bancodb, Ownerdb, codigoEntidade, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet sincronizaRecursosMSProject(char retornaValores)
    {
        comandoSQL = string.Format(@"EXEC {0}.{1}.p_msp2007_SincronizaRecursos '{2}'
                    ", bancodb, Ownerdb, retornaValores);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public bool incluiNovoEmailRecurso(string codigoRecurso, string email, int codigoEntidade, int codigoUsuarioInclusao, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    DECLARE @CodigoNovoUsuario Int, 
                            @CodigoRetorno Int

                    SET @CodigoNovoUsuario = 0
                    SET @CodigoRetorno = 0

                    EXEC {0}.{1}.p_msp2007_SincronizaUsuario '{2}', '{3}', {4}, {5}, @CodigoNovoUsuario output, @CodigoRetorno output

                    SELECT @CodigoRetorno AS CodigoRetorno
                END
                     ", bancodb, Ownerdb, codigoRecurso, email, codigoEntidade, codigoUsuarioInclusao);

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]) && ds.Tables[0].Rows[0]["CodigoRetorno"].ToString() == "2")
            {
                msgError = "Email já vinculado a outro recurso!";
                return false;
            }

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }


    public bool existeFilhosDoGrupoRecurso(int codigoGrupoSuperior)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                        SELECT  1 
                        FROM    {0}.{1}.GrupoRecurso
                        WHERE   GrupoRecursoSuperior = {2}
                        ", bancodb, Ownerdb, codigoGrupoSuperior);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                retorno = true;
            }
            else
            {
                retorno = false;
            }
        }
        catch
        {
            retorno = false;

        }
        return retorno;
    }

    public bool existeGrupoRecursoComMesmoNomeNaEntidade(string nomeGrupo, int codigoEntidade, int codigoGrupoRecursoAtual, string modo, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            if (modo == "I")
            {
                comandoSQL = string.Format(@"
            SELECT [DescricaoGrupo]
              FROM {0}.{1}.[GrupoRecurso]
             WHERE DescricaoGrupo like '{2}'
               AND CodigoEntidade = {3}", bancodb, Ownerdb, nomeGrupo, codigoEntidade);
            }
            else
            {
                comandoSQL = string.Format(@"
            SELECT [DescricaoGrupo]
              FROM {0}.{1}.[GrupoRecurso]
             WHERE DescricaoGrupo like '{2}'
               AND CodigoEntidade = {3} and CodigoGrupoRecurso != {4}", bancodb, Ownerdb, nomeGrupo, codigoEntidade, codigoGrupoRecursoAtual);
            }
            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                msgErro = string.Format((modo == "I") ? "Não é permitido incluir grupos com mesmo nome na entidade atual." : "Já existe outro grupo na entidade atual com esse mesmo nome.");
                retorno = true;
            }
            else
            {
                retorno = false;
            }
        }
        catch (SqlException ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool incluiGrupoRecurso(string nomeGrupo, int? codigoGrupoSuperior, string detalhe, int codigoEntidade, int codigoTipoRecursoGrupo,
                               string unidadeMedida, string valorHora, string valorUso, out int codigoNovoGrupo, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        codigoNovoGrupo = 0;
        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                    DECLARE @CodigoNovoGrupo Int, @Nivel Int

                    SET @Nivel = NULL
                    SELECT @Nivel = [NivelGrupo] FROM {0}.{1}.[GrupoRecurso] WHERE [CodigoGrupoRecurso] = {4}
                    IF (@Nivel IS NULL)
                        SET @Nivel = 1
                    ELSE
                        SET @Nivel = @Nivel + 1

                    INSERT INTO {0}.{1}.GrupoRecurso (DescricaoGrupo, DetalheGrupo, GrupoRecursoSuperior, NivelGrupo, CodigoEntidade, CodigoTipoRecurso, CustoHora, CustoUso, UnidadeMedida)
                                               VALUES(         '{2}',          {3},                  {4},     @Nivel,            {5}, {6}, {7}, {8}, {9})

                    SET @codigoNovoGrupo = SCOPE_IDENTITY()

                    SELECT @CodigoNovoGrupo AS CodigoNovoGrupo
                END
                     ", bancodb, Ownerdb, nomeGrupo,
                      (detalhe == null ? "NULL" : "'" + detalhe + "'"),
                      (codigoGrupoSuperior.HasValue ? codigoGrupoSuperior.Value.ToString() : "NULL"), codigoEntidade, codigoTipoRecursoGrupo,
                      valorHora, valorUso, unidadeMedida);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                codigoNovoGrupo = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool AtualizaGrupoRecurso(int codigoGrupo, string nomeGrupo, int? codigoGrupoSuperior, string detalhe, int codigoTipoRecursoGrupo, string unidadeMedida, string valorHora, string valorUso, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"
                    DECLARE @Nivel Int

                    SET @Nivel = NULL
                    SELECT @Nivel = [NivelGrupo] FROM {0}.{1}.[GrupoRecurso] WHERE [CodigoGrupoRecurso] = {4}
                    IF (@Nivel IS NULL)
                        SET @Nivel = 1
                    ELSE
                        SET @Nivel = @Nivel + 1

                    UPDATE {0}.{1}.GrupoRecurso SET 
	                    DescricaoGrupo = '{2}',
	                    DetalheGrupo   = {3},
	                    GrupoRecursoSuperior  = {4},
	                    NivelGrupo     = @Nivel,
                        CodigoTipoRecurso = {5},
                        CustoHora = {7}, 
                        CustoUso = {8}, 
                        UnidadeMedida = {9}
                    WHERE CodigoGrupoRecurso  = {6}
                     ", bancodb, Ownerdb, nomeGrupo,
                      (detalhe == null ? "NULL" : "'" + detalhe + "'"),
                      (codigoGrupoSuperior.HasValue ? codigoGrupoSuperior.Value.ToString() : "NULL"), codigoTipoRecursoGrupo, codigoGrupo,
                      valorHora, valorUso, unidadeMedida);

            int registrosAfetados = 0;
            execSQL(comandoSQL, ref registrosAfetados);
            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool excluiGrupoRecurso(string codigoGrupoAtual, string codigoEntidade, ref string mensagem)
    {
        int registrosAfetados = 0;
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            comandoSQL = string.Format(@"DELETE FROM {0}.{1}.GrupoRecurso
                                               WHERE CodigoGrupoRecurso = {2} 
                                                 AND CodigoEntidade = {3} ", bancodb, Ownerdb, codigoGrupoAtual, codigoEntidade);
            execSQL(comandoSQL, ref registrosAfetados);
        }
        catch (Exception ex)
        {
            mensagem = ex.Message;
            retorno = false;
        }
        return retorno;
    }

    public DataSet getTiposRecursos(string where)
    {
        string comandoSQL = string.Format(@"
        SELECT CodigoTipoRecurso, DescricaoTipoRecurso
        FROM    {0}.{1}.TipoRecurso
        WHERE   1=1
           {2}

        ORDER BY DescricaoTipoRecurso
        ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }


    //todo: Consultar con Ericson a mudança da linha [INNER JOIN  {0}.{1}.TipoAssociacao AS ta ON (ta.CodigoTipoAssociacao = ac.CodigoTipoAssociacao AND ta.IniciaisTipoAssociacao = 'RC')] 
    //                                           por [LEFT JOIN   {0}.{1}.TipoAssociacao AS ta ON (ta.CodigoTipoAssociacao = ac.CodigoTipoAssociacao AND ta.IniciaisTipoAssociacao = 'RC')]
    public DataSet getRecursosCorporativos(string codigoEntidade, string select, string where)
    {
        string comandoSQL = string.Format(@"
            SELECT rc.CodigoRecursoCorporativo
                    , rc.CodigoTipoRecurso
                    , rc.IndicaRecursoAtivo
                    , CASE IndicaRecursoAtivo  WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS estaAtivo
                    , rc.CodigoUsuario
                    , rc.CodigoGrupoRecurso
                    , rc.CodigoUnidadeNegocio
                    , rc.CustoHora
                    , rc.CustoUso
                    , rc.InicioDisponibilidadeRecurso
                    , rc.TerminoDisponibilidadeRecurso
                    , rc.Anotacoes
                    , us.NomeUsuario
                    , rc.NomeRecursoCorporativo
                    , rc.CodigoEntidade
                    , ac.CodigoCalendario, gr.DescricaoGrupo     
                    , rc.UnidadeMedidaRecurso
                    , un.NomeUnidadeNegocio
                    , tr.DescricaoTipoRecurso  
                    , rc.IndicaRecursoGenerico as Generico 
                    , CASE rc.IndicaRecursoGenerico WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS IndicaRecursoGenerico
                    , rc.IndicaEquipe as Equipe
                    , CASE rc.IndicaEquipe  WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS IndicaEquipe                  
		    {3}
                        FROM {0}.{1}.RecursoCorporativo      AS rc
                  INNER JOIN {0}.{1}.GrupoRecurso            AS gr ON (rc.CodigoGrupoRecurso = gr.CodigoGrupoRecurso)    
                  LEFT JOIN {0}.{1}.Usuario                 AS us ON us.CodigoUsuario = rc.CodigoUsuario
                  INNER JOIN {0}.{1}.AssociacaoCalendario    AS ac ON ac.CodigoObjetoAssociado = rc.CodigoRecursoCorporativo  
                  INNER JOIN {0}.{1}.TipoAssociacao          AS ta ON (ta.CodigoTipoAssociacao = ac.CodigoTipoAssociacao AND ta.IniciaisTipoAssociacao = 'RC')           
                  INNER JOIN {0}.{1}.Calendario AS c ON (c.CodigoCalendario = ac.CodigoCalendario 
                                               AND c.CodigoEntidade = rc.CodigoEntidade)
                  LEFT JOIN {0}.{1}.UnidadeNegocio          AS un ON (un.CodigoUnidadeNegocio =  rc.CodigoUnidadeNegocio) 
                 INNER JOIN {0}.{1}.TipoRecurso             AS tr ON (tr.CodigoTipoRecurso = rc.CodigoTipoRecurso)      
                WHERE rc.CodigoEntidade = {2}
                  AND us.DataExclusao IS NULL              
                  AND rc.CodigoTipoRecurso = 1 
                  AND (EXISTS (SELECT 1 
                                FROM {0}.{1}.UsuarioUnidadeNegocio   AS uun 
							   WHERE us.CodigoUsuario = uun.CodigoUsuario 
							   AND uun.CodigoUnidadeNegocio = rc.CodigoEntidade 
							   AND uun.IndicaUsuarioAtivoUnidadeNegocio = 'S') OR rc.IndicaEquipe = 'S' OR rc.IndicaRecursoGenerico = 'S')
              
                  {4}
                
    UNION
    SELECT rc.CodigoRecursoCorporativo
                    , rc.CodigoTipoRecurso
                    , rc.IndicaRecursoAtivo
                    , CASE IndicaRecursoAtivo  WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS estaAtivo
                    , rc.CodigoUsuario
                    , rc.CodigoGrupoRecurso
                    , rc.CodigoUnidadeNegocio
                    , rc.CustoHora
                    , rc.CustoUso
                    , rc.InicioDisponibilidadeRecurso
                    , rc.TerminoDisponibilidadeRecurso
                    , rc.Anotacoes
                    , NULL
                    , rc.NomeRecursoCorporativo
                    , rc.CodigoEntidade
                    , NULL
                    , gr.DescricaoGrupo
                    , rc.UnidadeMedidaRecurso
                    , un.NomeUnidadeNegocio 
                    , tr.DescricaoTipoRecurso  
                    , rc.IndicaRecursoGenerico as Generico 
                    , CASE rc.IndicaRecursoGenerico WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS IndicaRecursoGenerico
                    , rc.IndicaEquipe as Equipe
                    , CASE rc.IndicaEquipe  WHEN 'S'    THEN 'Sim' 
                                                           ELSE 'Não' END AS IndicaEquipe          
                    {3}
                FROM        {0}.{1}.RecursoCorporativo      AS rc
                  INNER JOIN  {0}.{1}.GrupoRecurso           AS gr ON (rc.CodigoGrupoRecurso = gr.CodigoGrupoRecurso)    
                  LEFT JOIN {0}.{1}.UnidadeNegocio          AS un ON (un.CodigoUnidadeNegocio =  rc.CodigoUnidadeNegocio)
                  INNER JOIN {0}.{1}.TipoRecurso            AS tr ON (tr.CodigoTipoRecurso = rc.CodigoTipoRecurso)       
                WHERE rc.CodigoEntidade = {2}
                  AND rc.CodigoTipoRecurso <> 1
                  {4}                
            

                ORDER BY rc.NomeRecursoCorporativo

            ", bancodb, Ownerdb, codigoEntidade, select, where);
        return getDataSet(comandoSQL);
    }

    public bool VerificaExisteRecursoCorporativo(int codigoTipoRecurso, string indicaRecursoAtivo, string codigoUsuario,
                                           int codigoGrupoRecurso, string codigoUnidadeNegocio, string custoHora,
                                           string custoUso, string inicioDisponibilidadeRecurso,
                                           string terminoDisponibilidadeRecurso, string anotacoes,
                                           string usuarioLogado, string unidadeMedida, string nomeRecurso, int codigoEntidade,
                                           ref string mensagem)
    {
        bool retorno = false;
        string comandoSQL = "";
        string data = "";
        if (codigoTipoRecurso == 1 && codigoUsuario == "")
        {
            comandoSQL = string.Format(@"
                    BEGIN
                              select  convert(varchar(20),max(DataInclusao),113)
	                                from RecursoCorporativo rc 
                                    where IndicaRecursoAtivo = 'S'
	                                and [CodigoUnidadeNegocio] = {6}  and 
	                                [CodigoTipoRecurso]  = {2} and 
	                                [CodigoGrupoRecurso] =  {5} and 
	                                [NomeRecursoCorporativo] = '{7}' 
                      
                    END
                ", bancodb, Ownerdb, codigoTipoRecurso, indicaRecursoAtivo
                     , codigoUsuario == "" ? "NULL" : codigoUsuario
                     , codigoGrupoRecurso, codigoUnidadeNegocio, nomeRecurso
                   );
        }
        else if (codigoTipoRecurso == 1 && codigoUsuario != "")
        {
            comandoSQL = string.Format(@"
                    BEGIN
                                select convert(varchar(20),max(DataInclusao),113)
	                                from RecursoCorporativo rc 
                                    where IndicaRecursoAtivo = 'S'
	                                and [CodigoUnidadeNegocio] = {6}  and 
	                                [CodigoTipoRecurso]  = {2} and 
	                                [CodigoGrupoRecurso] =  {5} and 
	                                [NomeRecursoCorporativo] = '{7}' and 
                                    [codigousuario] = {4} 
                      
                    END
                ", bancodb, Ownerdb, codigoTipoRecurso, indicaRecursoAtivo
                     , codigoUsuario == "" ? "NULL" : codigoUsuario
                     , codigoGrupoRecurso, codigoUnidadeNegocio, nomeRecurso
                   );
        }
        else
        {
            comandoSQL = string.Format(@"
                    BEGIN
                                select convert(varchar(20),max(DataInclusao),113)
	                                from RecursoCorporativo rc 
                                    where IndicaRecursoAtivo = 'S'
	                                and [CodigoUnidadeNegocio] = {6}  and 
	                                [CodigoTipoRecurso]  = {2} and 
	                                [CodigoGrupoRecurso] =  {5} and 
	                                [NomeRecursoCorporativo] = '{7}' 
                      
                    END
                ", bancodb, Ownerdb, codigoTipoRecurso, indicaRecursoAtivo
                     , codigoUsuario == "" ? "NULL" : codigoUsuario
                     , codigoGrupoRecurso, codigoUnidadeNegocio, nomeRecurso
                   );
        }

        try
        {
            DataSet ds = getDataSet(comandoSQL);
            //qtd = int.Parse(ds.Tables[0].Rows[0][0].ToString());
            data = ds.Tables[0].Rows[0][0].ToString();
            mensagem = data != "" ? "Consta para este Recurso Corporativo uma inclusão efetuada em: " + data + ", certifique-se de não ter dado duplo clique no botão salvar." : "";
            retorno = data != "";
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagem = ex.Message;
        }
        return retorno;
    }

    public bool incluirRecursoCorporativos(int codigoTipoRecurso, string indicaRecursoAtivo, string codigoUsuario,
                                           int codigoGrupoRecurso, string codigoUnidadeNegocio, string custoHora,
                                           string custoUso, string inicioDisponibilidadeRecurso,
                                           string terminoDisponibilidadeRecurso, string anotacoes,
                                           string usuarioLogado, string unidadeMedida, string nomeRecurso, int codigoEntidade,
                                           string IndicaGenerico, string IndicaEquipe,
                                           ref string codigoNovoRecurso, ref string codigoErro, ref string mensagemErro)
    {
        //bool retorno = false;
        //int regAfetados = 0;
        DataSet ds = null;
        string execProc = "";

        if (codigoTipoRecurso == 1 || codigoTipoRecurso == 2)
        {
            execProc = string.Format("EXEC {0}.{1}.p_incluiJobAtualizaCapacidadeDiariaRecurso @CodigoRecurso", bancodb, Ownerdb);
        }

        if (IndicaEquipe == "S" || IndicaGenerico == "S")
        {
            codigoUsuario = "";
        }

        string comandoSQL = string.Format(@"BEGIN
	                DECLARE @TranCounter INT;

	                SET @TranCounter = @@TRANCOUNT;

	                IF @TranCounter > 0
		                SAVE TRANSACTION savePoint_RecursoCorporativo;
	                ELSE
		                BEGIN TRANSACTION;

	                BEGIN TRY
		                DECLARE @CodigoRecurso INT
			                ,@CodigoEntidad INT
			                ,@ou_codigoRetorno INT
			                ,@ou_msgRetorno VARCHAR(4000)

		                SET @CodigoEntidad = {14}

		                IF NOT EXISTS (
				                SELECT 1
				                FROM RecursoCorporativo rc
				                INNER JOIN usuario u ON rc.CodigoUsuario = u.CodigoUsuario
					                AND u.dataExclusao IS NULL
				                WHERE IndicaRecursoAtivo = 'S'
					                AND [CodigoUnidadeNegocio] = {6}
					                AND [CodigoTipoRecurso] = {2}
					                AND [CodigoGrupoRecurso] = {5}
					                AND [NomeRecursoCorporativo] = '{13}'
					                AND rc.codigousuario = {4}
				                )
		                BEGIN
			                INSERT INTO {0}.{1}.RecursoCorporativo (
				                CodigoTipoRecurso
				                ,IndicaRecursoAtivo
				                ,CodigoUsuario
				                ,CodigoGrupoRecurso
				                ,CodigoUnidadeNegocio
				                ,CustoHora
				                ,CustoUso
				                ,InicioDisponibilidadeRecurso
				                ,TerminoDisponibilidadeRecurso
				                ,Anotacoes
				                ,DataInclusao
				                ,CodigoUsuarioInclusao
				                ,NomeRecursoCorporativo
				                ,CodigoEntidade
				                ,UnidadeMedidaRecurso
                                ,IndicaRecursoGenerico
                                ,IndicaEquipe
				                )
			                VALUES (
				                {2}
				                ,'{3}'
				                ,{4}
				                ,{5}
				                ,{6}
				                ,{7}
				                ,{8}
				                ,{9}
				                ,{10}
				                ,{11}
				                ,GETDATE()
				                ,{12}
				                ,'{13}'
				                ,@CodigoEntidad
				                ,{16}
                                ,'{17}'
                                ,'{18}' 
				                )

			                SET @CodigoRecurso = SCOPE_IDENTITY() 

                           {15}
		                END

                         -- Se for o caso, faz o COMMIT	
							IF (@@TRANCOUNT > 0)
                            BEGIN 
							  SET @ou_codigoRetorno = 0;
		                      SET @ou_msgRetorno = '';
                            END
  						  COMMIT TRANSACTION;
						  SELECT @CodigoRecurso AS [Codigo], @ou_codigoRetorno as codigoRetorno, @ou_msgRetorno as msgRetorno
		                
	                END TRY

	                BEGIN CATCH
		                SET @ou_codigoRetorno = ERROR_NUMBER();
		                SET @ou_msgRetorno = ERROR_MESSAGE();
                          
		                -- rollback transação caso a transação tenha sido criada aqui
		                IF @TranCounter = 0
			                ROLLBACK TRANSACTION;
		                ELSE
		                BEGIN
			                -- se a transação foi criada antes e ainda continua válida, dá rollback até 
			                -- o ponto criado nesta proc
			                IF XACT_STATE() <> - 1
				                ROLLBACK TRANSACTION savePoint_RecursoCorporativo;
		                END -- ELSE 
                        SELECT @CodigoRecurso AS [Codigo], @ou_codigoRetorno as codigoRetorno , @ou_msgRetorno as msgRetorno

	                END CATCH;
                END
                ", bancodb, Ownerdb, codigoTipoRecurso, indicaRecursoAtivo
                 , codigoUsuario == "" ? "NULL" : codigoUsuario
                 , codigoGrupoRecurso, codigoUnidadeNegocio
                 , custoHora, custoUso
                 , (inicioDisponibilidadeRecurso == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioDisponibilidadeRecurso + "', 103)")
                 , (terminoDisponibilidadeRecurso == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoDisponibilidadeRecurso + "', 103)")
                 , anotacoes
                 , usuarioLogado
                 , nomeRecurso, codigoEntidade, execProc, unidadeMedida, IndicaGenerico, IndicaEquipe);

        ds = getDataSet(comandoSQL);
        codigoNovoRecurso = ds.Tables[0].Rows[0]["Codigo"].ToString();
        codigoErro = ds.Tables[0].Rows[0]["codigoRetorno"].ToString();


        switch (int.Parse(codigoErro))
        {
            case 2812:
                mensagemErro = "Não foi possível calcular a capacidade diária do Recurso";
                break;
            default:
                mensagemErro = ds.Tables[0].Rows[0]["msgRetorno"].ToString();
                break;
        }

        return (int.Parse(codigoErro) == 0);
        //try
        //{
        //    ds = getDataSet(comandoSQL);
        //    codigoNovoRecurso = ds.Tables[0].Rows[0][0].ToString();
        //    retorno = true;
        //}
        //catch 
        //{

        //    codigoErro = ds.Tables[0].Rows[0][1].ToString();
        //    mensagemErro = ds.Tables[0].Rows[0][2].ToString();
        //    retorno = false;
        //}

    }

    public bool atualizaRecursoCorporativos(string codigoTipoRecurso, string indicaRecursoAtivo, string codigoUsuario,
                                       string codigoGrupoRecurso, string codigoUnidadeNegocio, string custoHora,
                                       string custoUso, string inicioDisponibilidadeRecurso,
                                       string terminoDisponibilidadeRecurso, string anotacoes,
                                       int usuarioLogado, string nomeRecurso, string unidadeMedida, string codigoEntidade, string chave, string IndicaGenerico, string IndicaEquipe)
    {
        bool retorno = false;
        int regAfetados = 0;
        string ativaOuInativaUsuario = "";

        if(indicaRecursoAtivo == "S")
        {
            ativaOuInativaUsuario = "DataDesativacaoRecurso = null, CodigoUsuarioDesativacaoRecurso = null";
        }
        else
        {
            ativaOuInativaUsuario = "DataDesativacaoRecurso = GETDATE(), CodigoUsuarioDesativacaoRecurso = " + usuarioLogado;
        }

        string comandoSQL = string.Format(@"

                    UPDATE {0}.{1}.RecursoCorporativo SET
                                                                    {18}
                            ,    CodigoTipoRecurso               =   {2}
                            ,   IndicaRecursoAtivo              =   '{3}'
                           -- ,   CodigoUsuario                   =   {4}
                            ,   CodigoGrupoRecurso              =   {5}
                            ,   CodigoUnidadeNegocio            =   {6}
                            ,   CustoHora                       =   {7}
                            ,   CustoUso                        =   {8}
                            ,   InicioDisponibilidadeRecurso    =   {9}
                            ,   TerminoDisponibilidadeRecurso   =   {10}
                            ,   Anotacoes                       =   {11}
                            ,   NomeRecursoCorporativo          =   '{12}'
                            ,   DataUltimaAlteracao             =   GETDATE()
                            ,   CodigoUsuarioUltimaAlteracao    =   {13}
                            ,   UnidadeMedidaRecurso            =   {15}
                            ,   IndicaRecursoGenerico           =   '{16}'
                            ,   IndicaEquipe                    =   '{17}' 

                    WHERE CodigoRecursoCorporativo = {14}

                ", bancodb, Ownerdb, codigoTipoRecurso, indicaRecursoAtivo
                 , codigoUsuario == "" ? "NULL" : codigoUsuario
                 , codigoGrupoRecurso, codigoUnidadeNegocio
                 , custoHora, custoUso
                 , (inicioDisponibilidadeRecurso == "NULL" ? "NULL" : "CONVERT(DateTime, '" + inicioDisponibilidadeRecurso + "', 103)")
                 , (terminoDisponibilidadeRecurso == "NULL" ? "NULL" : "CONVERT(DateTime, '" + terminoDisponibilidadeRecurso + "', 103)")
                 , anotacoes
                 , nomeRecurso
                 , usuarioLogado, chave, unidadeMedida, IndicaGenerico, IndicaEquipe, ativaOuInativaUsuario);

        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    /// <summary>
    /// Deleta recurso corporativo indicado como parâmetro.
    /// Deleta toda referência de calendário que tubiera o recurso corporativo..
    /// </summary>
    /// <param name="codigoRecursoCorporativo">Código do Recurso corporativo que será excluido.</param>
    /// <returns></returns>
    public bool excluiRecursoCorporativos(string codigoRecursoCorporativo)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
        DECLARE @CodigoRecursoCorporativo   Int
	        ,   @CodigoCalendarioRecurso    Int

	        SET @CodigoRecursoCorporativo   = {2}

	        SELECT  @CodigoCalendarioRecurso	= ac.[CodigoCalendario]
		        FROM        {0}.{1}.[AssociacaoCalendario]  AS [ac]
				INNER JOIN  {0}.{1}.[TipoAssociacao]        AS [ta]			ON 
					        (ta.[CodigoTipoAssociacao]	= ac.[CodigoTipoAssociacao])
		        WHERE   ta.[IniciaisTipoAssociacao] = 'RC'
			        AND ac.[CodigoObjetoAssociado]      = @CodigoRecursoCorporativo
            			

			BEGIN TRAN

	        BEGIN TRY
	            DELETE {0}.{1}.[DetalheCalendarioDiaSemana]
	            FROM        {0}.{1}.[DetalheCalendarioDiaSemana]    AS [tab]
            	INNER JOIN  {0}.{1}.[Calendario]                    AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendarioBase]		= @CodigoCalendarioRecurso
            		
	            DELETE {0}.{1}.[CalendarioDiaSemana]
	            FROM        {0}.{1}.[CalendarioDiaSemana]	AS [tab]
                INNER JOIN  {0}.{1}.[Calendario]			AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendarioBase]		= @CodigoCalendarioRecurso
            		
	            DELETE {0}.{1}.[AssociacaoCalendario]
	            FROM        {0}.{1}.[AssociacaoCalendario]	AS [tab]
			    INNER JOIN  {0}.{1}.[Calendario]			AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendarioBase]		= @CodigoCalendarioRecurso
            		
                DELETE {0}.{1}.[CalendarioProjecao] WHERE [CodigoCalendario]	= @CodigoCalendarioRecurso

	            DELETE {0}.{1}.[Calendario] WHERE [CodigoCalendarioBase]	= @CodigoCalendarioRecurso

	            DELETE {0}.{1}.[DetalheCalendarioDiaSemana]
	            FROM        {0}.{1}.[DetalheCalendarioDiaSemana]    AS [tab]
                INNER JOIN  {0}.{1}.[Calendario]                    AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendario]		= @CodigoCalendarioRecurso
            		
	            DELETE  {0}.{1}.[CalendarioDiaSemana]
	            FROM        {0}.{1}.[CalendarioDiaSemana]	AS [tab]
            	INNER JOIN  {0}.{1}.[Calendario]			AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendario]		= @CodigoCalendarioRecurso
            		
	            DELETE {0}.{1}.[AssociacaoCalendario]
	            FROM        {0}.{1}.[AssociacaoCalendario]	AS [tab]
            	INNER JOIN  {0}.{1}.[Calendario]			AS [cal]		ON 
				            (cal.[CodigoCalendario]			= tab.[CodigoCalendario])
	            WHERE   cal.[CodigoCalendario]		= @CodigoCalendarioRecurso
            		
	            DELETE {0}.{1}.[Calendario]         WHERE [CodigoCalendario]        = @CodigoCalendarioRecurso
            	DELETE {0}.{1}.[CapacidadeAlocacaoDiariaRecurso] WHERE [CodigoRecursoCorporativo]= @CodigoRecursoCorporativo
            	DELETE {0}.{1}.[RecursoCorporativo] WHERE [CodigoRecursoCorporativo]= @CodigoRecursoCorporativo
            	COMMIT
            END TRY
            BEGIN CATCH
							DECLARE 
									@ErrorMessage			Nvarchar(4000)
								, @ErrorSeverity		Int
								, @ErrorState				Int
								, @ErrorNumber			Int;

							SET @ErrorMessage		= ERROR_MESSAGE();
							SET @ErrorSeverity	= ERROR_SEVERITY();
							SET @ErrorState			= ERROR_STATE();
							SET @ErrorNumber		= ERROR_NUMBER();
            	ROLLBACK TRANSACTION
							RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
            END CATCH
        ", bancodb, Ownerdb, codigoRecursoCorporativo);
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception)
        {
            retorno = false;
        }
        return retorno;
    }

    public string verificaExclusaoRecursoCorporativo(string codigoRecursoCorporativo, string tipo)
    {
        string msgRetorno = "";

        string op = tipo.Equals("I") ? "inativado" : "excluído";

        string comandoSQL = "";
        string comandoSQL_Agil = "";
        if (tipo.Equals("I")) //Inativar
        {
            comandoSQL = string.Format(@"
            SELECT distinct p.NomeProjeto 
              FROM {0}.{1}.RecursoCronogramaProjeto rcp INNER JOIN
			       {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = rcp.CodigoCronogramaProjeto INNER JOIN
			       {0}.{1}.Projeto p ON p.CodigoProjeto = cp.CodigoProjeto inner join 
			       {0}.{1}.AtribuicaoRecursoTarefa art on (art.CodigoCronogramaProjeto = rcp.CodigoCronogramaProjeto and 
                                                   art.CodigoRecursoProjeto = rcp.CodigoRecursoProjeto and 
                                                   art.TerminoReal is null)
             WHERE CodigoRecursoCorporativo = '{2}' 
               AND (p.DataExclusao is null and 
                    p.CodigoStatusProjeto not in (4,6))", bancodb, Ownerdb, codigoRecursoCorporativo);

            comandoSQL_Agil = string.Format(@"
            SELECT DISTINCT p.[NomeProjeto]
            FROM  {0}.{1}.[Agil_RecursoIteracao]   ari INNER JOIN
                  {0}.{1}.[Agil_Iteracao]   ai  ON (ai.[CodigoIteracao] = ari.[CodigoIteracao]) INNER JOIN
                  {0}.{1}.[LinkProjeto]   lp ON (lp.[CodigoProjetoFilho] = ai.[CodigoProjetoIteracao] AND lp.TipoLink = 'PJPJ') INNER JOIN
                  {0}.{1}.[Projeto]   p ON (p.[CodigoProjeto] = lp.[CodigoProjetoFilho])
            WHERE ari.[CodigoRecursoCorporativo] = {2}
            AND p.[DataExclusao] IS NULL
            AND p.[CodigoStatusProjeto] NOT IN (4,6)", bancodb, Ownerdb, codigoRecursoCorporativo);
        }

        else if (tipo.Equals("E")) //Excluir
        {
            comandoSQL = string.Format(@"
            SELECT distinct p.NomeProjeto 
              FROM {0}.{1}.RecursoCronogramaProjeto rcp INNER JOIN
			       {0}.{1}.CronogramaProjeto cp ON cp.CodigoCronogramaProjeto = rcp.CodigoCronogramaProjeto INNER JOIN
			       {0}.{1}.Projeto p ON p.CodigoProjeto = cp.CodigoProjeto
             WHERE CodigoRecursoCorporativo = '{2}'", bancodb, Ownerdb, codigoRecursoCorporativo);

            comandoSQL_Agil = string.Format(@"
            SELECT DISTINCT p.[NomeProjeto]
            FROM {0}.{1}.[Projeto]  p INNER JOIN
                 {0}.{1}.[Agil_RecursoProjeto]  arp ON (arp.[CodigoProjeto] = p.[CodigoProjeto])
            WHERE arp.[CodigoRecursoCorporativo] = {2}
            AND p.[DataExclusao] IS NULL
            AND p.[CodigoStatusProjeto] NOT IN (4,6)", bancodb, Ownerdb, codigoRecursoCorporativo);
        }

        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            msgRetorno = "O recurso não pode ser " + op + ", porque está alocado nos cronogramas e/ou sprints dos seguintes projetos:";

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                msgRetorno += Environment.NewLine;
                msgRetorno += "   - " + dr["NomeProjeto"].ToString();
            }
            msgRetorno += Environment.NewLine;
        }

        ds = getDataSet(comandoSQL_Agil);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            if (msgRetorno.Length == 0)
            {
                msgRetorno = "O recurso não pode ser " + op + ", porque está alocado nas sprints dos seguintes projetos:";
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                msgRetorno += Environment.NewLine;
                msgRetorno += "   - " + dr["NomeProjeto"].ToString();
            }
            msgRetorno += Environment.NewLine;
        }
        return msgRetorno;
    }

    public bool AtualizaCapacidadeDiariaRecurso(int codigoRecurso, ref string msgErro)
    {
        try
        {
            int regAfetados = 0;
            string comandoSQL = string.Format(@"
                DECLARE @RC int
                DECLARE @CodigoRecursoParam int

                SET @CodigoRecursoParam = {2}

                EXECUTE @RC = {0}.{1}.[p_AtualizaCapacidadeDiariaRecurso] 
                @CodigoRecursoParam
            ", bancodb, Ownerdb, codigoRecurso);
            execSQL(comandoSQL, ref regAfetados);
            return true;
        }
        catch (Exception ex)
        {
            msgErro = ex.Message;
            return false;
        }
    }

    #endregion

    #region administracao - adm_ConfiguracaoPessoais.aspx



    public DataSet getURLTelaInicialUsuario(string codigoEntidade, string codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                SELECT {0}.{1}.f_GetURLDashPadraoUsuario({2}) AS TelaInicial
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getMapaEstrategicoConfiguracaoPessoais(string codigoEntidade, string codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                SELECT  me.CodigoMapaEstrategico,
                        me.TituloMapaEstrategico 
                FROM    {0}.{1}.f_GetMapasEstrategicosUsuario({2}, {3}) AS f 
                    INNER JOIN {0}.{1}.MapaEstrategico AS me 
                        ON (me.CodigoMapaEstrategico = f.CodigoMapaEstrategico)
                WHERE me.IndicaMapaEstrategicoAtivo = 'S' 
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }


    public DataSet getMapaEstrategicoComImagemConfiguracaoPessoais(string codigoEntidade, string codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                SELECT  me.CodigoMapaEstrategico,
                        me.TituloMapaEstrategico 
                FROM    {0}.{1}.f_GetMapasEstrategicosUsuario({2}, {3}) AS f 
                    INNER JOIN {0}.{1}.MapaEstrategico AS me 
                        ON (me.CodigoMapaEstrategico = f.CodigoMapaEstrategico)
                WHERE me.IndicaMapaEstrategicoAtivo = 'S' 
                       and ImagemMapa is not null
                ", bancodb, Ownerdb, codigoUsuario, codigoEntidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getEntidadeConfiguracaoPessoais(string codigoUsuario)
    {
        string comandoSQL = string.Format(@"
                SELECT  u.CodigoUsuario, u.NomeUsuario, 
                        uun.CodigoUnidadeNegocio, uun.IndicaUsuarioAtivoUnidadeNegocio,
                        un.NomeUnidadeNegocio

                FROM {0}.{1}.Usuario AS u
                    INNER JOIN {0}.{1}.UsuarioUnidadeNegocio AS uun
                        ON uun.CodigoUsuario = u.CodigoUsuario

                    INNER JOIN {0}.{1}.UnidadeNegocio AS un
                        ON un.CodigoEntidade = uun.CodigoUnidadeNegocio

                WHERE u.CodigoUsuario = {2}
                ", bancodb, Ownerdb, codigoUsuario);
        return getDataSet(comandoSQL);
    }



    #endregion

    #region administracao - adm_TipoReuniao.aspx

    public bool getExisteNomeTipoReuniaoEntidadAtual(int codigoEntidad, string nomeTipoReuniao, int codigoTipoReuniao)
    {
        bool retorno = true;
        string commandoSql = string.Format(@"
                SELECT CodigoTipoEvento, DescricaoTipoEvento 
                FROM {0}.{1}.TipoEvento
                WHERE CodigoEntidade = {2}
                  AND DescricaoTipoEvento = '{3}'
                ", bancodb, Ownerdb, codigoEntidad
                 , nomeTipoReuniao.Replace("'", "''"));

        DataSet ds = getDataSet(commandoSql);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            if (codigoTipoReuniao == -1)
                retorno = false;
            else
            {
                int codigoBanco = int.Parse(ds.Tables[0].Rows[0]["CodigoTipoEvento"].ToString());
                if (codigoTipoReuniao != codigoBanco)
                    retorno = false;
            }
        }

        return retorno;
    }

    #endregion

    #region administracao - Calendários

    public DataSet getCalendariosEntidade(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(@"
                BEGIN        
                    DECLARE @CodigoTipoAssociacao int
	
	                SELECT @CodigoTipoAssociacao = ta.CodigoTipoAssociacao 
	                  FROM TipoAssociacao ta
	                 WHERE ta.IniciaisTipoAssociacao = 'EN'
    
                    SELECT c.CodigoCalendario, c.DescricaoCalendario, c.IndicaCalendarioPadrao
                      FROM {0}.{1}.Calendario c INNER JOIN
                           {0}.{1}.AssociacaoCalendario ac ON (ac.CodigoCalendario = c.CodigoCalendario AND ac.CodigoObjetoAssociado = {2} AND ac.CodigoTipoAssociacao = @CodigoTipoAssociacao)
                     WHERE c.CodigoEntidade = {2}
                       AND c.IndicaTipoCalendario = 'C'
                       {3}
                END
                ", bancodb, Ownerdb, codigoEntidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCalendario(int codigoCalendario, string where)
    {
        string comandoSQL = string.Format(@"
                BEGIN            
                    SELECT c.CodigoCalendario, c.DescricaoCalendario, c.IndicaCalendarioPadrao, c.HorasDia, c.HorasSemana, c.DiasMes
                      FROM {0}.{1}.Calendario c 
                     WHERE c.CodigoCalendario = {2}
                       AND c.IndicaTipoCalendario = 'C'
                       {3}
                END
                ", bancodb, Ownerdb, codigoCalendario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCodigoCalendarioAssociado(int codigoObjetoAssociado, string tipoAssociacao, string where)
    {
        string comandoSQL = string.Format(@"
                BEGIN
	               DECLARE @CodigoTipoAssociacao int
                	
	                SELECT @CodigoTipoAssociacao = ta.CodigoTipoAssociacao 
	                  FROM {0}.{1}.TipoAssociacao ta
	                 WHERE ta.IniciaisTipoAssociacao = '{3}'
                	
                    SELECT ac.CodigoCalendario
                      FROM {0}.{1}.AssociacaoCalendario ac
                     WHERE ac.CodigoTipoAssociacao = @CodigoTipoAssociacao
                       AND ac.CodigoObjetoAssociado = {2}
                   
                  END
                ", bancodb, Ownerdb, codigoObjetoAssociado, tipoAssociacao, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiCopiaCalendario(int? codigoCalendarioBase, int codigoEntidade, int codigoUsuarioInclusao, string tipoAssociacao, int codigoObjetoAssociado, string nomeCalendario, ref int codigoNovoCalendario)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {
            string calendarioBase = "";

            if (codigoCalendarioBase.HasValue)
            {
                calendarioBase = "SET @CodigoCalendarioBase = " + codigoCalendarioBase.Value;
            }
            else
            {
                calendarioBase = string.Format(@"
                                SELECT @CodigoCalendarioBase = CodigoCalendario 
                                FROM    {0}.{1}.Calendario
                                WHERE   IndicaCalendarioPadrao  = 'S'
                                  AND   CodigoEntidade          = {2}
                                ", bancodb, Ownerdb, codigoEntidade);
            }

            comandoSQL = string.Format(@"
                    BEGIN
                        DECLARE @codigoNovoCalendario int,
                                @CodigoTipoAssociacao int,
                                @CodigoCalendarioBase int,
                                @HorasDia int,
                                @HorasSemana int,
                                @DiasMes int
                        
                        {7}

                        SELECT @HorasDia = HorasDia, @HorasSemana = HorasSemana, @DiasMes = DiasMes
                          FROM  {0}.{1}.Calendario 
                         WHERE CodigoCalendario = @CodigoCalendarioBase 

                        SELECT @CodigoTipoAssociacao = ta.CodigoTipoAssociacao 
	                      FROM {0}.{1}.TipoAssociacao ta
	                     WHERE ta.IniciaisTipoAssociacao = '{5}'

                        INSERT INTO {0}.{1}.Calendario
                        (   DescricaoCalendario     , IndicaCalendarioPadrao        , CodigoEntidade    , CodigoCalendarioBase
                        ,   IndicaTipoCalendario    , CodigoUsuarioInclusao         , DataInclusao      , HorasDia
                        ,   HorasSemana             , DiasMes)
                        VALUES
                        (   '{8}'                   , 'N'                           , {2}               , @CodigoCalendarioBase
                        ,   'C'                     , {4}                           , GETDATE()         , @HorasDia
                        ,   @HorasSemana            , @DiasMes)	

                        SET @codigoNovoCalendario = SCOPE_IDENTITY()
                        ", bancodb, Ownerdb, codigoEntidade, codigoCalendarioBase, codigoUsuarioInclusao, tipoAssociacao
                         , codigoObjetoAssociado, calendarioBase, nomeCalendario);

            if (nomeCalendario == "")
            {
                comandoSQL += string.Format(@"
                        UPDATE {0}.{1}.Calendario 
                           SET DescricaoCalendario = '" + Resources.traducao.novo_calend_rio.ToString() + @" ' + CONVERT(VarChar, @codigoNovoCalendario)
                         WHERE CodigoCalendario = @codigoNovoCalendario
                        ", bancodb, Ownerdb);
            }

            comandoSQL += string.Format(@"      
                        INSERT INTO {0}.{1}.AssociacaoCalendario(CodigoCalendario, CodigoTipoAssociacao, CodigoObjetoAssociado)
                                                          VALUES(@codigoNovoCalendario, @CodigoTipoAssociacao, {6})
                            
                        INSERT INTO {0}.{1}.CalendarioDiaSemana
                            SELECT @codigoNovoCalendario    , DiaSemana         , HoraInicioTurno1      , HoraTerminoTurno1
                                ,   HoraInicioTurno2        , HoraTerminoTurno2 , HoraInicioTurno3      , HoraTerminoTurno3
                                ,   HoraInicioTurno4        , HoraTerminoTurno4 , IndicaHorarioPadrao
                             FROM {0}.{1}.CalendarioDiaSemana 
                            WHERE CodigoCalendario = @CodigoCalendarioBase
                                                       
                        INSERT INTO {0}.{1}.DetalheCalendarioDiaSemana
                            SELECT @codigoNovoCalendario, DiaSemana, Data, IndicaDiaUtil
                             FROM {0}.{1}.DetalheCalendarioDiaSemana 
                            WHERE CodigoCalendario = @CodigoCalendarioBase

                        --------- inserir dias das exceções.
                        DECLARE 
                                @DescricaoCalendarioExcecao     VarChar(60)
                            ,   @CodigoCalendarioExcecao        Int
                            ,   @CodigoNovoCalendarioExcecao    Int

                        DECLARE crsCalendariosExcecoes CURSOR LOCAL FAST_FORWARD FOR 
                            SELECT  CodigoCalendario, DescricaoCalendario
                            FROM    {0}.{1}.Calendario
                            WHERE   CodigoCalendarioBase = @CodigoCalendarioBase
                              AND   IndicaTipoCalendario = 'E'

                        OPEN crsCalendariosExcecoes

                        FETCH NEXT FROM crsCalendariosExcecoes INTO @CodigoCalendarioExcecao, @DescricaoCalendarioExcecao
                        -----------------------------------------------------------------------------------------

                        -----------------------------------------------------------------------------------------
                        --------- LOOP para processamento de cada linha 
                        WHILE (@@FETCH_STATUS = 0) BEGIN	

                        INSERT INTO {0}.{1}.Calendario 
                        (   DescricaoCalendario         , IndicaCalendarioPadrao            , CodigoEntidade
                        ,   CodigoCalendarioBase        , IndicaTipoCalendario              , CodigoUsuarioInclusao
                        ,   DataInclusao)
                        VALUES
                        (   @DescricaoCalendarioExcecao , 'N'                               , {2}
                        ,   @codigoNovoCalendario       , 'E'                               , {4}
                        ,   GETDATE() )

                        SET @CodigoNovoCalendarioExcecao = SCOPE_IDENTITY()

                        INSERT INTO {0}.{1}.CalendarioDiaSemana
                        SELECT  @CodigoNovoCalendarioExcecao    , DiaSemana         , HoraInicioTurno1      , HoraTerminoTurno1
                            ,   HoraInicioTurno2                , HoraTerminoTurno2 , HoraInicioTurno3      , HoraTerminoTurno3
                            ,   HoraInicioTurno4                , HoraTerminoTurno4 , IndicaHorarioPadrao
                        FROM    {0}.{1}.CalendarioDiaSemana 
                        WHERE   CodigoCalendario = @CodigoCalendarioExcecao												

                        INSERT INTO {0}.{1}.DetalheCalendarioDiaSemana
                        SELECT  @CodigoNovoCalendarioExcecao    , DiaSemana , Data  , IndicaDiaUtil
                        FROM    {0}.{1}.DetalheCalendarioDiaSemana 
                        WHERE   CodigoCalendario = @CodigoCalendarioExcecao												

                        FETCH NEXT FROM crsCalendariosExcecoes INTO @CodigoCalendarioExcecao, @DescricaoCalendarioExcecao

                        END -- WHILE @@FETCH_STATUS = 0 
                        --------- FIM DO LOOP para tratamento de cada linha 
                        --------------------------------------------------------------------------------------------------------
                        CLOSE crsCalendariosExcecoes
                        DEALLOCATE crsCalendariosExcecoes

                        EXECUTE {0}.{1}.[p_AtualizaCapacidadeDiariaRecurso] {6}

                        SELECT @codigoNovoCalendario As codigoCalendario
                    END
                   ", bancodb, Ownerdb, codigoEntidade, codigoCalendarioBase, codigoUsuarioInclusao, tipoAssociacao
                    , codigoObjetoAssociado, calendarioBase
                    , nomeCalendario == "" ? "" : nomeCalendario.Trim().Replace("'", "''"));

            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                codigoNovoCalendario = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool getExisteNomeCalendarioEntidadAtual(int codigoEntidad, string nomeCalendario, int codigoCalendario)
    {
        bool retorno = true;
        string commandoSql = string.Format(@"
                SELECT CodigoCalendario, DescricaoCalendario 
                FROM {0}.{1}.Calendario
                where CodigoEntidade = {2}
                AND DescricaoCalendario = '{3}'
                ", bancodb, Ownerdb, codigoEntidad
                 , nomeCalendario.Replace("'", "''"));

        DataSet ds = getDataSet(commandoSql);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            int codigoBanco = int.Parse(ds.Tables[0].Rows[0]["CodigoCalendario"].ToString());
            if (codigoCalendario != codigoBanco)
                retorno = false;
        }

        return retorno;
    }

    public bool atualizaCalendario(int codigoCalendario, string descricaoCalendario, int horasDia, int horasSemana, int diasMes, int codigoUsuarioAlteracao
        , string[,] segunda, string[,] terca, string[,] quarta, string[,] quinta, string[,] sexta, string[,] sabado, string[,] domingo, int codigoRecurso)
    {
        bool retorno = false;
        string comandoSQL = "", comandoHorariosPadrao = "";
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN                        
                        DECLARE @RC int
                        DECLARE @CodigoRecursoParam int                       
                        begin tran
                        DELETE FROM {0}.{1}.CalendarioDiaSemana WHERE CodigoCalendario = {3}
                        
                        UPDATE {0}.{1}.Calendario 
                           SET  DescricaoCalendario          = '{2}'
                            ,   HorasDia = {5}
                            ,   HorasSemana = {6}
                            ,   DiasMes = {7}
                            ,   CodigoUsuarioUltimaAlteracao = {4}
                            ,   DataUltimaAlteracao          = GetDate()
                         WHERE CodigoCalendario = {3}
                
                         SET @CodigoRecursoParam = {8}

                       
                   ", bancodb, Ownerdb
                    , descricaoCalendario.Replace("'", "''")
                    , codigoCalendario, codigoUsuarioAlteracao
                    , horasDia
                    , horasSemana
                    , diasMes, codigoRecurso);

            int regAf = 0;

            comandoHorariosPadrao += montaInsertHorarios(2, codigoCalendario.ToString(), segunda, 'S');
            comandoHorariosPadrao += montaInsertHorarios(3, codigoCalendario.ToString(), terca, 'S');
            comandoHorariosPadrao += montaInsertHorarios(4, codigoCalendario.ToString(), quarta, 'S');
            comandoHorariosPadrao += montaInsertHorarios(5, codigoCalendario.ToString(), quinta, 'S');
            comandoHorariosPadrao += montaInsertHorarios(6, codigoCalendario.ToString(), sexta, 'S');
            comandoHorariosPadrao += montaInsertHorarios(7, codigoCalendario.ToString(), sabado, 'S');
            comandoHorariosPadrao += montaInsertHorarios(1, codigoCalendario.ToString(), domingo, 'S');

            comandoHorariosPadrao += string.Format(@"   EXECUTE @RC = {0}.{1}.[p_AtualizaCapacidadeDiariaRecurso] @CodigoRecursoParam ", bancodb, Ownerdb);
            comandoSQL = comandoSQL + " " + comandoHorariosPadrao + @"
                        IF @@error <> 0 
                            ROLLBACK TRAN
                        ELSE
                            COMMIT
                        END";

            execSQL(comandoSQL, ref regAf);
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaExcecao(int codigoExcecao, string descricaoCalendario, int codigoUsuarioAlteracao
        , string[,] segunda, string[,] terca, string[,] quarta, string[,] quinta, string[,] sexta, string[,] sabado, string[,] domingo
        , DateTime dataInicio, DateTime dataTermino)
    {
        bool retorno = false;
        string comandoSQL = "", comandoHorariosPadrao = "";
        try
        {
            comandoSQL = string.Format(@"
                    BEGIN                        
                      BEGIN TRAN
                          DECLARE @DataInicio               DateTime,
                                  @DataFim                  DateTime,
                                  @CodigoCalendarioExcecao  Int,
                                  @DataControle             DateTime,
                                  @IndicaDiaUtil            Char(1)

                            SET @CodigoCalendarioExcecao    = {3}
                            SET @DataInicio                 = CONVERT(DateTime, '{5:dd/MM/yyyy}', 103)
                            SET @DataFim                    = CONVERT(DateTime, '{6:dd/MM/yyyy}', 103)

                            DELETE FROM {0}.{1}.DetalheCalendarioDiaSemana  WHERE CodigoCalendario = {3}
                            
                            DELETE FROM {0}.{1}.CalendarioDiaSemana         WHERE CodigoCalendario = {3}
                            
                            UPDATE  {0}.{1}.Calendario 
                               SET  DescricaoCalendario          = '{2}'
                                ,   CodigoUsuarioUltimaAlteracao = {4}
                                ,   DataUltimaAlteracao          = GetDate()
                             WHERE CodigoCalendario = {3}
                   ", bancodb, Ownerdb
                    , descricaoCalendario.Trim().Replace("'", "''")
                    , codigoExcecao, codigoUsuarioAlteracao, dataInicio, dataTermino);

            int regAf = 0;

            comandoHorariosPadrao += montaInsertHorarios(2, codigoExcecao.ToString(), segunda, 'N');
            comandoHorariosPadrao += montaInsertHorarios(3, codigoExcecao.ToString(), terca, 'N');
            comandoHorariosPadrao += montaInsertHorarios(4, codigoExcecao.ToString(), quarta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(5, codigoExcecao.ToString(), quinta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(6, codigoExcecao.ToString(), sexta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(7, codigoExcecao.ToString(), sabado, 'N');
            comandoHorariosPadrao += montaInsertHorarios(1, codigoExcecao.ToString(), domingo, 'N');

            comandoSQL = comandoSQL + " " + comandoHorariosPadrao;

            comandoSQL += string.Format(@"
                            SET @DataControle = @DataInicio

                            WHILE @DataControle <= @DataFim
                                BEGIN
                                    /* Verifica se o dia atual foi colocado como dia útil */
                                    SET @IndicaDiaUtil = Null

                                    SELECT @IndicaDiaUtil = CASE    WHEN HoraInicioTurno1 IS NULL THEN 'N'
                                                                    ELSE 'S' END             
                                    FROM    {0}.{1}.CalendarioDiaSemana
                                    WHERE   CodigoCalendario    = @CodigoCalendarioExcecao 
                                      AND   DiaSemana           = DATEPART(WEEKDAY,@DataControle)

                                    /* Se trouxe algum registro, inclui na tabela de detalhes */   
                                    IF @IndicaDiaUtil IS NOT NULL  
                                        INSERT INTO {0}.{1}.DetalheCalendarioDiaSemana
                                        (   CodigoCalendario            , Data          , DiaSemana                         , IndicaDiaUtil     )
                                        VALUES
                                        (   @CodigoCalendarioExcecao    , @DataControle , DATEPART(WEEKDAY,@DataControle)   , @IndicaDiaUtil    )

                                    SET @DataControle = @DataControle + 1
                                END      

                            IF @@error <> 0 
                                ROLLBACK TRAN
                            ELSE
                                COMMIT

                            END
                        ", bancodb, Ownerdb);

            execSQL(comandoSQL, ref regAf);
            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    public bool incluiExcecao(int codigoEntidade, int codigoCalendarioBase, string descricaoCalendario, int codigoUsuarioInclusao
        , string[,] segunda, string[,] terca, string[,] quarta, string[,] quinta, string[,] sexta, string[,] sabado, string[,] domingo
        , DateTime dataInicio, DateTime dataTermino)
    {
        bool retorno = false;
        string comandoSQL = "", comandoHorariosPadrao = "";
        try
        {


            comandoSQL = string.Format(@"
                    BEGIN                        

                      DECLARE @DataInicio DateTime, 
                              @DataFim DateTime,
                              @CodigoCalendarioExcecao Int,
                              @DataControle DateTime,          
                              @IndicaDiaUtil Char(1)

                        
                        SET @DataInicio = CONVERT(DateTime, '{6:dd/MM/yyyy}', 103)
                        SET @DataFim    = CONVERT(DateTime, '{7:dd/MM/yyyy}', 103)                        
                        
                        INSERT INTO {0}.{1}.Calendario
                        (   DescricaoCalendario         , IndicaCalendarioPadrao        , CodigoEntidade        , CodigoCalendarioBase
                        ,   IndicaTipoCalendario        , CodigoUsuarioInclusao         , DataInclusao  )
                        VALUES
                        (   '{2}'                       , 'N'                           , {3}                   , {4}
                        ,   'E'                         , {5}                           , GETDATE()     )
                               
                         
                        SET @CodigoCalendarioExcecao = scope_identity()
                   ", bancodb, Ownerdb
                    , descricaoCalendario.Trim().Replace("'", "''")
                    , codigoEntidade, codigoCalendarioBase, codigoUsuarioInclusao, dataInicio, dataTermino
                    );

            int regAf = 0;

            comandoHorariosPadrao += montaInsertHorarios(2, "@CodigoCalendarioExcecao", segunda, 'N');
            comandoHorariosPadrao += montaInsertHorarios(3, "@CodigoCalendarioExcecao", terca, 'N');
            comandoHorariosPadrao += montaInsertHorarios(4, "@CodigoCalendarioExcecao", quarta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(5, "@CodigoCalendarioExcecao", quinta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(6, "@CodigoCalendarioExcecao", sexta, 'N');
            comandoHorariosPadrao += montaInsertHorarios(7, "@CodigoCalendarioExcecao", sabado, 'N');
            comandoHorariosPadrao += montaInsertHorarios(1, "@CodigoCalendarioExcecao", domingo, 'N');

            comandoSQL = comandoSQL + " " + comandoHorariosPadrao;

            comandoSQL += string.Format(@"
                        SET @DataControle = @DataInicio

                        WHILE @DataControle <= @DataFim
                            BEGIN
                            /* Verifica se o dia atual foi colocado como dia útil */
                            SET @IndicaDiaUtil = Null

                            SELECT @IndicaDiaUtil = CASE    WHEN HoraInicioTurno1 IS NOT NULL THEN 'S'
                                                            WHEN HoraInicioTurno2 IS NOT NULL THEN 'S'
                                                            WHEN HoraInicioTurno3 IS NOT NULL THEN 'S'
                                                            WHEN HoraInicioTurno4 IS NOT NULL THEN 'S'
                                                            ELSE 'N' END             
                            FROM    {0}.{1}.CalendarioDiaSemana
                            WHERE   CodigoCalendario    = @CodigoCalendarioExcecao 
                              AND   DiaSemana           = DATEPART(WEEKDAY,@DataControle)

                            /* Se trouxe algum registro, inclui na tabela de detalhes */   
                            IF @IndicaDiaUtil IS NOT NULL  
                                INSERT INTO {0}.{1}.DetalheCalendarioDiaSemana
                                (   CodigoCalendario            , Data          , DiaSemana                         , IndicaDiaUtil  )
                                VALUES
                                (   @CodigoCalendarioExcecao    , @DataControle ,   DATEPART(WEEKDAY,@DataControle) , @IndicaDiaUtil ) 
                            
                            SET @DataControle = @DataControle + 1

                            END      
                        END
                        ", bancodb, Ownerdb);

            execSQL(comandoSQL, ref regAf);

            retorno = true;
        }
        catch
        {
            retorno = false;
        }
        return retorno;
    }

    private string montaInsertHorarios(int diaSemana, string codigoCalendario, string[,] horarios, char indicaHorarioPadrao)
    {
        string iniTurno1, iniTurno2, iniTurno3, iniTurno4, termTurno1, termTurno2, termTurno3, termTurno4;

        iniTurno1 = horarios[0, 0].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[0, 0] + ":00', 103)";
        termTurno1 = horarios[0, 1].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[0, 1] + ":00', 103)";
        iniTurno2 = horarios[1, 0].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[1, 0] + ":00', 103)";
        termTurno2 = horarios[1, 1].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[1, 1] + ":00', 103)";
        iniTurno3 = horarios[2, 0].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[2, 0] + ":00', 103)";
        termTurno3 = horarios[2, 1].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[2, 1] + ":00', 103)";
        iniTurno4 = horarios[3, 0].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[3, 0] + ":00', 103)";
        termTurno4 = horarios[3, 1].Replace(":", "").Trim() == "" ? "NULL" : "CONVERT(DateTime, '01/01/2000 " + horarios[3, 1] + ":00', 103)";

        string comando = string.Format(@"INSERT INTO {0}.{1}.CalendarioDiaSemana(CodigoCalendario, DiaSemana, HoraInicioTurno1, HoraTerminoTurno1, 
                                                                         HoraInicioTurno2, HoraTerminoTurno2, 
                                                                         HoraInicioTurno3, HoraTerminoTurno3, 
                                                                         HoraInicioTurno4, HoraTerminoTurno4, IndicaHorarioPadrao)
                                                                VALUES({2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, '{12}');"
            , bancodb, Ownerdb, codigoCalendario, diaSemana, iniTurno1, termTurno1, iniTurno2, termTurno2, iniTurno3, termTurno3, iniTurno4, termTurno4, indicaHorarioPadrao);

        return comando;
    }


    public DataSet getHorariosDataSelecionada(int codigoCalendario, string data, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DISTINCT HoraInicio, HoraTermino, Turno
                  FROM {0}.{1}.f_GetHorarioCalendario({2}, CONVERT(DateTime, '{3}', 103))
                 WHERE 1 = 1
                   {4}
                ", bancodb, Ownerdb, codigoCalendario, data, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getExcecoesCalendario(int codigoCalendario, string where)
    {
        string comandoSQL = string.Format(@"
                      SELECT c.CodigoCalendario AS CodigoExcecao,
                             DescricaoCalendario AS Excecao,
                             MIN(dcd.Data) AS Inicio,
                             MAX(dcd.Data) AS Termino
                        FROM {0}.{1}.Calendario AS c INNER JOIN
                             {0}.{1}.DetalheCalendarioDiaSemana AS dcd ON (dcd.CodigoCalendario = c.CodigoCalendario)
                       WHERE c.CodigoCalendarioBase = {2}    
                          {3}  
                     GROUP BY c.CodigoCalendario, c.DescricaoCalendario  
                  
                ", bancodb, Ownerdb, codigoCalendario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDiasMesCalendario(int codigoCalendario, int ano, int mes, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DAY(Data) AS Dia,
                       IndicaDiaUtil        
                  FROM DetalheCalendarioDiaSemana
                 WHERE YEAR(Data) = {3}
                   AND MONTH(Data) = {4}
                   AND CodigoCalendario IN (SELECT CodigoCalendario 
                              FROM {0}.{1}.Calendario AS c
                             WHERE c.CodigoCalendarioBase = {2})
                   {5}
                ", bancodb, Ownerdb, codigoCalendario, ano, mes, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getHorariosPadroesCalendario(int codigoCalendario, string where)
    {
        string comandoSQL = string.Format(@"
                SELECT DiaSemana, HoraInicioTurno1, HoraInicioTurno2, HoraInicioTurno3, HoraInicioTurno4,
	                   HoraTerminoTurno1, HoraTerminoTurno2, HoraTerminoTurno3, HoraTerminoTurno4
                  FROM {0}.{1}.CalendarioDiaSemana
                 WHERE CodigoCalendario = {2}                   
                   {3}
                ", bancodb, Ownerdb, codigoCalendario, where);
        return getDataSet(comandoSQL);
    }

    public bool excluiCalendario(int codigoCalendario)
    {
        string comandoSQL = "";
        DataTable dt = null;
        bool retorno = false;

        try
        {
            comandoSQL = string.Format(@"
                        BEGIN

                        DECLARE @CodigoCalendarioBase  INT

                        SET @CodigoCalendarioBase = {2}

                        IF EXISTS   (
                                    SELECT  1 
                                    FROM    {0}.{1}.Calendario
                                    WHERE   CodigoCalendarioBase = @CodigoCalendarioBase
                                      AND   IndicaTipoCalendario = 'C'
                                    )
                            SELECT 1 AS Retorno
                        ELSE 
                            BEGIN
                                DELETE FROM {0}.{1}.CalendarioProjecao WHERE CodigoCalendario =  @CodigoCalendarioBase
                                
                                DELETE FROM {0}.{1}.DetalheCalendarioDiaSemana 
                                WHERE       CodigoCalendario        = @CodigoCalendarioBase
                                   OR       CodigoCalendario IN (SELECT CodigoCalendario 
                                                                   FROM Calendario 
                                                                  WHERE CodigoCalendarioBase = @CodigoCalendarioBase);

                                DELETE FROM {0}.{1}.CalendarioDiaSemana 
                                WHERE       CodigoCalendario        = @CodigoCalendarioBase
                                   OR       CodigoCalendario IN (SELECT CodigoCalendario 
                                                                   FROM Calendario 
                                                                  WHERE CodigoCalendarioBase = @CodigoCalendarioBase);

                                DELETE FROM {0}.{1}.AssociacaoCalendario 
                                WHERE       CodigoCalendario        = @CodigoCalendarioBase;

                                DELETE FROM {0}.{1}.Calendario 
                                WHERE       CodigoCalendarioBase    = @CodigoCalendarioBase;

                                DELETE FROM {0}.{1}.Calendario 
                                WHERE       CodigoCalendario        = @CodigoCalendarioBase;

                                SELECT 0 AS Retorno
                            END
                        END
            "
            , bancodb, Ownerdb, codigoCalendario);


            dt = getDataSet(comandoSQL).Tables[0];
            if (DataTableOk(dt))
                retorno = dt.Rows[0]["Retorno"].ToString().Equals("0");
        }
        catch (Exception ex)
        {
            throw new Exception(getErroExcluiRegistro(ex.Message));
        }

        return retorno;
    }

    public bool verificaNecessidadeProjecaoCalendario(int codigoCalendario)
    {
        bool bRet = true;
        string comandoSQL = "";
        int qtdRegAft = 0;

        comandoSQL = string.Format(@"
        BEGIN
            DECLARE @CodigoCalendario Int, @RetornoProcessamento Int

            SET @CodigoCalendario = {2}

            EXEC {0}.{1}.p_verificaNecessidadeProjecaoCalendario
		          @in_codigoCalendario			= @CodigoCalendario
	            , @ou_CodigoProcessamento		= @RetornoProcessamento OUTPUT
        END "
        , bancodb, Ownerdb, codigoCalendario);

        execSQL(comandoSQL, ref qtdRegAft);

        return bRet;
    }

    #endregion

    #region administracao - CadastroPerfil

    public DataSet getPerfil(string where, string orderBy)
    {
        //observação: public DataSet getPerfisEntidade(){...}
        string comandoSQL = string.Format(@"
                SELECT  p.CodigoPerfil
                    ,   p.CodigoEntidade
                    ,   p.CodigoTipoAssociacao
                    ,   p.IniciaisPerfil
                    ,   p.DescricaoPerfil_PT
                    ,   p.ObservacaoPerfil
                    ,   p.StatusPerfil
                    ,   ta.DescricaoTipoAssociacao
                    ,   ta.IniciaisTipoAssociacao

                FROM        {0}.{1}.Perfil                      AS p
                INNER JOIN  {0}.{1}.TipoAssociacao              AS ta   ON p.CodigoTipoAssociacao = ta.CodigoTipoAssociacao
                INNER JOIN	{0}.{1}.HierarquiaTipoAssociacao    AS hta  ON p.CodigoTipoAssociacao = hta.CodigoTipoAssociacaoFilho 

                WHERE 1 = 1 --p.StatusPerfil != 'D'
                  {2}

                {3}
                ", bancodb, Ownerdb, where, orderBy);
        return getDataSet(comandoSQL);
    }

    public DataSet getTipoAssociacaoTelaPerfil(string where)
    {
        string comandoSQL = string.Format(@"
                SELECT  taf.CodigoTipoAssociacao
                    ,   taf.DescricaoTipoAssociacao
                    ,   hta.NivelHierarquicoFilho

                FROM        {0}.{1}.TipoAssociacao              AS taf
                INNER JOIN  {0}.{1}.HierarquiaTipoAssociacao    AS hta  ON  (taf.CodigoTipoAssociacao = hta.CodigoTipoAssociacaoFilho)

                WHERE 1=1
                   {2}

                ORDER BY hta.NivelHierarquicoFilho
               ", bancodb, Ownerdb, where);

        return getDataSet(comandoSQL);
    }

    //perfis

    public bool incluirPerfil(int idTipoAssociacao, string descricaoPerfil, string observacaoPerfil
                            , int idEntidade, int idUsuario, ref int codigoPerfil, ref string iniciaiPerfil
                            , ref string mensagemErro)
    {
        DataSet ds;
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                DECLARE @CodigoTipoAssociacao   INT
                    ,   @ObservacaoPerfil       VARCHAR(250)
                    ,   @RetornoIniciais        VARCHAR(2)

                SET @ObservacaoPerfil       = {4}
                SET @CodigoTipoAssociacao   = {2}

				IF EXISTS ( SELECT 1 From {0}.{1}.Perfil WHERE DescricaoPerfil_PT = '{3}' AND CodigoTipoAssociacao = @CodigoTipoAssociacao AND CodigoEntidade = {5})
					SELECT 'ERRO' AS Resultado
				ELSE
				BEGIN
                    INSERT INTO {0}.{1}.Perfil (    CodigoTipoAssociacao
                                                ,   DescricaoPerfil_PT
                                                ,   DescricaoPerfil_EN
                                                ,   DescricaoPerfil_ES
                                                ,   IniciaisPerfil
                                                ,   ObservacaoPerfil
                                                ,   CodigoEntidade
                                                ,   StatusPerfil
                                                ,   DataInclusao
                                                ,   CodigoUsuarioInclusao
                                                )
                    VALUES(     @CodigoTipoAssociacao
                            ,   '{3}'
                            ,   '{3}'
                            ,   '{3}'
                            ,   NULL
                            ,   @ObservacaoPerfil
                            ,   {5}
                            ,   'A'
                            ,   GETDATE()
                            ,   {6}
                            )

                    SELECT @RetornoIniciais = IniciaisTipoAssociacao FROM {0}.{1}.TipoAssociacao WHERE CodigoTipoAssociacao = {2}
                    
                    SELECT SCOPE_IDENTITY() AS CodigoPerfil, @RetornoIniciais AS IniciaiPerfil, 'OK' AS Resultado
            END
            ", bancodb, Ownerdb, idTipoAssociacao
             , descricaoPerfil.Trim().Replace("'", "''")
             , observacaoPerfil == "" ? "NULL" : "'" + observacaoPerfil.Trim().Replace("'", "''") + "'"
             , idEntidade
             , idUsuario);

            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                mensagemErro = ds.Tables[0].Rows[0]["Resultado"].ToString();
                if (mensagemErro.Equals("OK"))
                {
                    codigoPerfil = int.Parse(ds.Tables[0].Rows[0]["CodigoPerfil"].ToString());
                    iniciaiPerfil = ds.Tables[0].Rows[0]["IniciaiPerfil"].ToString();
                }

            }

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaPerfil(int idPerfil, string descricaoPerfil, string observacaoPerfil
                        , int idEntidade, ref string iniciaiPerfil, ref string mensagemErro)
    {
        DataSet ds;
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
            DECLARE @DescricaoPerfil    VARCHAR(50)
                ,   @ObservacaoPerfil   VARCHAR(250)
                ,   @CodigoPerfil       INT
                ,   @CodigoTipoObjeto   INT
                ,   @RetornoIniciais        VARCHAR(2)

            SET @DescricaoPerfil    = '{4}'
            SET @ObservacaoPerfil   = {5}
            SET @CodigoPerfil       = {2}
            SELECT @CodigoTipoObjeto = CodigoTipoAssociacao FROM PERFIL WHERE CodigoPerfil = @CodigoPerfil

            IF EXISTS(SELECT 1 From {0}.{1}.Perfil WHERE DescricaoPerfil_PT = @DescricaoPerfil AND CodigoPerfil != @CodigoPerfil AND CodigoTipoAssociacao = @CodigoTipoObjeto AND CodigoEntidade = {3})
                BEGIN
                    SELECT 'ERROR' AS Resultado
                END
            ELSE
                BEGIN
                    UPDATE {0}.{1}.Perfil
                    SET     DescricaoPerfil_PT  = @DescricaoPerfil
                        ,   DescricaoPerfil_EN  = @DescricaoPerfil
                        ,   DescricaoPerfil_ES  = @DescricaoPerfil
                        ,   ObservacaoPerfil    = @ObservacaoPerfil

                    WHERE CodigoPerfil      = @CodigoPerfil
                      AND CodigoEntidade    = {3}

                    SELECT @RetornoIniciais = IniciaisTipoAssociacao FROM {0}.{1}.TipoAssociacao AS ta INNER JOIN {0}.{1}.Perfil AS p ON ta.CodigoTipoAssociacao = p.CodigoTipoAssociacao WHERE p.CodigoPerfil = {2}
                    SELECT @RetornoIniciais AS IniciaiPerfil, 'OK' AS Resultado
            END
            ", bancodb, Ownerdb, idPerfil, idEntidade
             , descricaoPerfil.Trim().Replace("'", "''")
             , observacaoPerfil == "" ? "NULL" : "'" + observacaoPerfil.Trim().Replace("'", "''") + "'");

            ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
            {
                mensagemErro = ds.Tables[0].Rows[0]["Resultado"].ToString();
                iniciaiPerfil = ds.Tables[0].Rows[0]["IniciaiPerfil"].ToString();
            }

            retorno = true;
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }
        return retorno;
    }


    public bool excluiPerfil(int idPerfil, int idUsuario, int idEntidade, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
                UPDATE {0}.{1}.Perfil

                SET DataDesativacao             = GETDATE()
                ,	CodigoUsuarioDesativacao    = {2}
                ,   StatusPerfil                = 'D'

                WHERE CodigoPerfil = {3}
                AND CodigoEntidade = {4}

                ", bancodb, Ownerdb
                 , idUsuario, idPerfil, idEntidade);

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

    public bool desativarPerfil(int idPerfil, int idUsuario, int idEntidade, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
                UPDATE {0}.{1}.Perfil

                SET DataDesativacao             = GETDATE()
                ,	CodigoUsuarioDesativacao    = {2}
                ,   StatusPerfil                = 'D'

                WHERE CodigoPerfil = {3}
                AND CodigoEntidade = {4}

                ", bancodb, Ownerdb
                 , idUsuario, idPerfil, idEntidade);

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

    public bool reativarPerfil(int idPerfil, int idUsuario, int idEntidade, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
                UPDATE {0}.{1}.Perfil

                SET 
                    DataAtivacao            = GETDATE()
                ,   CodigoUsuarioAtivacao   = {2}
                ,   StatusPerfil            = 'A'

                WHERE CodigoPerfil = {3}
                AND CodigoEntidade = {4}

                ", bancodb, Ownerdb
                 , idUsuario, idPerfil, idEntidade);

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
    //permissões

    public bool incluirPermissoesPerfil(int idPerfil, string listaPermissoes, int idUsuario, ref string mensagemErro)
    {
        bool retorno = true;
        try
        {
            string comandoSQL = string.Format(@"
                    EXEC {0}.{1}.p_perm_registraPermissoesPerfil {2}, '{3}', {4}
            ", bancodb, Ownerdb, idPerfil, listaPermissoes, idUsuario);

            int regAfetados = 0;
            execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            mensagemErro = ex.Message;
        }

        return retorno;
    }

    #endregion

    #region administracaob - ListagemPerfisUsuario

    public DataSet getListagemPerfisUsuario(int idUsuarioLogado, int idEntidadeLogado, int idUsuarioAlvo)
    {
        string comandoSQL = string.Format(@"
                SELECT * 
                FROM {0}.{1}.f_GetAcessosUsuario({2}, {3}, {4}) AS f 
                ORDER BY f.[NivelHierarquicoObjeto], f.DescricaoObjeto
                ", getDbName(), getDbOwner(), idUsuarioAlvo, idEntidadeLogado, idUsuarioLogado);

        return getDataSet(comandoSQL);
    }

    #endregion

    #region administracao - GrandesDesafios

    public DataSet getGrandesDesafios(string where)
    {

        string comandoSQL = string.Format(@"
                SELECT [CodigoEntidade]
                      ,[CodigoGrandeDesafio]
                      ,[DescricaoGrandeDesafio]
                      ,[MetaPeriodoEstrategico]
                      ,[IndicaGrandeDesafioAtivo]
                  FROM {0}.{1}.[tai_GrandeDesafio] gd
                where 1 = 1 
                {2} 
                ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool incluiGrandeDesafio(string MetaPeriodo, string GrandeDesafio, string codigoEntidade, string indicaGrandeDesafioAtivo, ref string identityCodigoGrandeDesafio, ref string msgError)
    {
        bool retorno = false;
        string comandoSQL = "";
        try
        {

            DateTime agora = DateTime.Now;

            comandoSQL = string.Format(@"
                    DECLARE @codigoGrandeDesafio int

                    INSERT INTO {0}.{1}.[tai_GrandeDesafio] ([CodigoEntidade]
                                                            ,[DescricaoGrandeDesafio]
                                                            ,[MetaPeriodoEstrategico]
                                                            ,[IndicaGrandeDesafioAtivo])

                    VALUES({4}, '{3}', '{2}', '{5}')	
                            
                    SELECT @codigoGrandeDesafio = SCOPE_IDENTITY()
                    
                    SELECT @codigoGrandeDesafio As codigoGrandeDesafio
                   ", bancodb, Ownerdb, MetaPeriodo, GrandeDesafio, codigoEntidade, indicaGrandeDesafioAtivo);

            DataSet ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                identityCodigoGrandeDesafio = ds.Tables[0].Rows[0][0].ToString();

            retorno = true;
        }
        catch (Exception ex)
        {
            msgError = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    public bool atualizaGrandeDesafio(string MetaPeriodo, string GrandeDesafio, string indicaGrandeDesafioAtivo, string codigoGrandeDesafio)
    {
        bool retorno = false;
        string mensagem = "";
        string comandoSQL = "";
        int registrosAfetados = 0;

        try
        {
            comandoSQL = string.Format(@"
                BEGIN
                  UPDATE {0}.{1}.[tai_GrandeDesafio] 
                     SET [DescricaoGrandeDesafio] = '{2}',
                         [MetaPeriodoEstrategico] = '{3}',
                         [IndicaGrandeDesafioAtivo] = '{4}' 
                   WHERE [CodigoGrandeDesafio] = {5}
                   
                END
                ", bancodb, Ownerdb, GrandeDesafio, MetaPeriodo, indicaGrandeDesafioAtivo, codigoGrandeDesafio);

            execSQL(comandoSQL, ref registrosAfetados);

            retorno = true;
        }
        catch (Exception ex)
        {
            mensagem = ex.Message + Environment.NewLine + comandoSQL;
            retorno = false;
        }
        return retorno;
    }

    #endregion

    #region PDA - Produto
    public DataSet getUnidadeMedida(string where)
    {
        comandoSQL = string.Format(@"
                SELECT  codigoUnidadeMedidaRecurso
                       ,descricaoUnidadeMedidaRecurso + ' - ' + siglaUnidadeMedidaRecurso AS ComboUF
                       ,siglaUnidadeMedidaRecurso
                       ,descricaoUnidadeMedidaRecurso
                FROM {0}.{1}.[TipoUnidadeMedidaRecurso] 
                WHERE (1=1) 
                   {2}
                ORDER BY descricaoUnidadeMedidaRecurso", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }


    #endregion

    #region _Projetos - Dados do Projeto

    public bool atualizaStatusProjeto(int codigoProjeto, int idUsuarioLogado, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @Nome VarChar(50)

                SELECT @Nome = NomeUsuario 
                  FROM {0}.{1}.Usuario
                 WHERE CodigoUsuario = {3}
                
                EXEC {0}.{1}.p_AtualizaStatusProjetos @Nome, {2}
                
            END"
            , bancodb, Ownerdb, codigoProjeto, idUsuarioLogado);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool sincronizaStatusProjeto(int codigoProjeto, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"                
                EXEC {0}.{1}.p_SincronizaStatusProjeto {2}"
            , bancodb, Ownerdb, codigoProjeto);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public DataSet getDadosGeraisProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @" BEGIN
	                DECLARE @CodigoProjeto int       
	                SET @CodigoProjeto = {2}


	                SELECT c.DescricaoCategoria, 
	                       rp.CodigoProjeto, 
	                       p.NomeProjeto         AS NomeProjeto, 
	                       g.NomeUsuario          AS Gerente, 
	                       g.Email                AS EmailGerente, 
	                       u.SiglaUnidadeNegocio         AS Unidade, 
	                       rp.InicioLB             AS DataInicio,
	                       rp.TerminoLB            AS DataTermino,
	                       isNull(rp.CustoTendenciaTotal, 0)              AS CustoTendenciaTotal,
	                       rp.InicioReprogramado  as InicioReprogramado,
	                       rp.TerminoReprogramado as TerminoReprogramado,
	                       rp.TerminoReal         as TerminoReal,
                           rp.CorGeral, 
                           p.DataUltimaAtualizacao AS UltimaAtualizacao,
                           p.DataUltimaAtualizacao AS DataUltimaAtualizacao,
                           ISNULL(g.CodigoUsuario, -1) AS CodigoGerente, 
                           u.CodigoUnidadeNegocio, 
                           ISNULL(p.CodigoCategoria, -1) AS CodigoCategoria,
                           c.SiglaCategoria,
                           p.IndicaRecursosAtualizamTarefas,
                           p.IndicaTipoAtualizacaoTarefas,
                           p.IndicaAprovadorTarefas,
                           p.CodigoReservado,
                           DescricaoProposta,
                           p.CodigoMSProject,
                           p.InicioProposta,
                           p.TerminoProposta,
                           p.Memo1,
                           p.Valor1,
                           p.texto1,
                           p.CodigoTipoProjeto,
                           tp.TipoProjeto,
                           p.CodigoUnidadeAtendimento,
                           cp.CodigoCarteira,
                           cp.IndicaCarteiraPrincipal,
                           ca.NomeCarteira  
	                  FROM ({0}.{1}.Projeto AS p LEFT JOIN 
		                {0}.{1}.UnidadeNegocio AS u ON p.CodigoUnidadeNegocio = u.CodigoUnidadeNegocio) LEFT OUTER JOIN 
                        {0}.{1}.TipoProjeto AS tp ON p.CodigoTipoProjeto = tp.CodigoTipoProjeto LEFT OUTER JOIN
		                {0}.{1}.Usuario AS g ON p.CodigoGerenteProjeto = g.CodigoUsuario LEFT OUTER JOIN  
		                {0}.{1}.ResumoProjeto AS rp  ON rp.CodigoProjeto = p.CodigoProjeto LEFT OUTER JOIN 
		                {0}.{1}.Categoria AS c ON c.CodigoCategoria = p.CodigoCategoria LEFT OUTER JOIN
                        {0}.{1}.CarteiraProjeto as cp ON (cp.CodigoProjeto = p.CodigoProjeto and cp.IndicaCarteiraPrincipal = 'S') LEFT JOIN
                        {0}.{1}.Carteira ca ON (ca.Codigocarteira = cp.CodigoCarteira)
	                WHERE p.CodigoProjeto = @CodigoProjeto
                      {3}
               END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getItensProjetoAtualizacaoMonitorada(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @" BEGIN
	                DECLARE @CodigoProjeto int       
	                SET @CodigoProjeto = {2}

	                SELECT 
				                [CodigoProjeto]
			                , [IDItemAtualizacao]
			                , [DescricaoItemAtualizacao]
			                , [DescricaoUltimaAtualizacaoItem]
			                , [DataUltimaAtualizacaoItem]
			                , [IDUsuarioUltimaAtualizacao]
		                FROM 
			                {0}.{1}.f_ipam_getEventosAtualizacaoProjeto(@CodigoProjeto)
                        WHERE 1=1 {3}
                END	 ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosGeraisObra(int codigoProjeto, int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	
	                DECLARE @UtilizaCalculoSaldoPorValorPago Char(1)
		
		                SELECT @UtilizaCalculoSaldoPorValorPago = Valor
		                  FROM {0}.{1}.ParametroConfiguracaoSistema
		                 WHERE Parametro = 'calculaSaldoContratualPorValorContrato'
		                   AND CodigoEntidade = {2}
		   
	                SELECT p.NomeProjeto, rp.InicioLB AS InicioPrevisto, rp.TerminoLB AS TerminoPrevisto, p.DataUltimaAtualizacao as UltimaAtualizacao
				                ,rp.InicioReal, rp.TerminoReprogramado, m.NomeMunicipio, ISNULL(pe.NomePessoa, 'SEM CONTRATO') AS Contratada
				                ,c.NumeroContrato, c.DataInicio, ISNULL(c.DataTermino, c.DataTerminoOriginal) AS DataTermino, c.ValorContrato
				                ,( SELECT ISNULL(SUM(pc2.[ValorPrevisto]), 0) FROM {0}.{1}.[ParcelaContrato] AS [pc2] WHERE pc2.[CodigoContrato] = c.[CodigoContrato] AND pc2.[DataExclusao] IS NULL) AS [ValorMedido]
				                ,( SELECT c.[ValorContrato] - ISNULL(CASE WHEN ISNULL(@UtilizaCalculoSaldoPorValorPago, 'N') = 'S' THEN SUM(pc2.[ValorPago]) ELSE SUM(pc2.[ValorPrevisto]) END, 0) FROM {0}.{1}.[ParcelaContrato] AS [pc2] WHERE pc2.[CodigoContrato] = c.[CodigoContrato] AND pc2.[DataExclusao] IS NULL) AS [SaldoContratual]
		                FROM {0}.{1}.Projeto p LEFT JOIN
				             {0}.{1}.ResumoProjeto rp ON rp.CodigoProjeto = p.CodigoProjeto LEFT JOIN
				             {0}.{1}.Obra o ON o.CodigoProjeto = p.CodigoProjeto LEFT JOIN
				             {0}.{1}.Contrato c ON c.CodigoContrato = o.CodigoContrato LEFT JOIN
				             {0}.{1}.Municipio m ON m.CodigoMunicipio = o.CodigoMunicipioObra LEFT JOIN
				             {0}.{1}.Pessoa pe ON pe.CodigoPessoa = c.CodigoPessoaContratada LEFT JOIN 
                             {0}.{1}.[PessoaEntidade] AS [pen] ON (
			                pen.[CodigoPessoa] = c.[CodigoPessoaContratada]
			                AND pen.codigoEntidade = c.codigoEntidade
                            --AND pen.IndicaFornecedor = 'S'
			                ) 
                       WHERE p.CodigoProjeto = {3}
       
                END
               ", bancodb, Ownerdb, codigoEntidade, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFotosObra(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
            @"SELECT TOP 3 a.CodigoAnexo, ca.Anexo, a.DescricaoAnexo, ISNULL(a.dataCheckin, a.datainclusao) AS Data
				FROM {0}.{1}.Anexo AS a INNER JOIN
					 {0}.{1}.AnexoAssociacao AS aa ON (aa.CodigoAnexo = a.CodigoAnexo 
											  AND aa.CodigoTipoAssociacao = 4) INNER JOIN
					 {0}.{1}.AnexoVersao AS av ON (av.codigoAnexo = a.CodigoAnexo) INNER JOIN                                                                     
					 {0}.{1}.ConteudoAnexo AS ca ON (ca.codigoSequencialAnexo = av.codigoSequencialAnexo)						 
				WHERE aa.CodigoObjetoAssociado = {2}
                    AND a.DataExclusao IS NULL
					AND (av.nomeArquivo LIKE '%.JPG%' 
					OR av.NomeArquivo LIKE '%.BMP%'
					OR av.NomeArquivo LIKE '%.PNG%')
                ORDER BY ISNULL(a.dataCheckIn, DataInclusao) DESC
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUltimoComentarioFiscalizacaoObra(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT Top 1 ap.Analise, ap.DataInclusao, u.NomeUsuario
                  FROM {0}.{1}.AnalisePerformance AS ap INNER JOIN 
			           {0}.{1}.Usuario u ON u.CodigoUsuario = ap.CodigoUsuarioInclusao
                 WHERE ap.CodigoObjetoAssociado = {2}
                   AND ap.CodigoTipoAssociacao = 4
                 ORDER BY DataInclusao DESC
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getObjetivosDisponiveisProjeto(int codigoProjeto, int codigoEntidade, int codigoUsuario, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoTipoObjeto int,
			                @CodigoProjeto int,
			                @CodigoEntidade int,
			                @CodigoUsuario int
			
	                SET @CodigoProjeto = {2}
	                SET @CodigoEntidade = {3}
	                SET @CodigoUsuario = {4}
	
	                SELECT @CodigoTipoObjeto = CodigoTipoObjetoEstrategia	
	                  FROM {0}.{1}.TipoObjetoEstrategia
	                 WHERE IniciaisTipoObjeto = 'OBJ'
	 
	                SELECT CodigoObjetoEstrategia, DescricaoObjetoEstrategia
	                  FROM {0}.{1}.ObjetoEstrategia oe
	                 WHERE oe.DataExclusao IS NULL
	                   AND oe.CodigoTipoObjetoEstrategia = @CodigoTipoObjeto
	                   AND oe.CodigoMapaEstrategico IN(SELECT f.CodigoMapaEstrategico 
									                     FROM {0}.{1}.f_GetMapasEstrategicosUsuario(@CodigoUsuario, @CodigoEntidade) f)
	                  AND oe.CodigoObjetoEstrategia NOT IN(SELECT poe.CodigoObjetivoEstrategico 
					                                        FROM {0}.{1}.ProjetoObjetivoEstrategico poe 
					                                       WHERE poe.CodigoProjeto = @CodigoProjeto) 
                END", bancodb, Ownerdb, codigoProjeto, codigoEntidade, codigoUsuario, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getObjetivosAssociadosProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoTipoObjeto int,
			                @CodigoProjeto int
			
	                SET @CodigoProjeto = {2}
	
	                SELECT @CodigoTipoObjeto = CodigoTipoObjetoEstrategia	
	                  FROM {0}.{1}.TipoObjetoEstrategia
	                 WHERE IniciaisTipoObjeto = 'OBJ'
	 
	                SELECT CodigoObjetoEstrategia, DescricaoObjetoEstrategia, IndicaObjetivoEstrategicoPrincipal
	                  FROM {0}.{1}.ObjetoEstrategia oe INNER JOIN
						   {0}.{1}.ProjetoObjetivoEstrategico poe  ON (poe.CodigoObjetivoEstrategico = oe.CodigoObjetoEstrategia
															   AND poe.CodigoProjeto = @CodigoProjeto)
	                 WHERE oe.DataExclusao IS NULL
	                   AND oe.CodigoTipoObjetoEstrategia = @CodigoTipoObjeto 
                     ORDER BY IndicaObjetivoEstrategicoPrincipal DESC, DescricaoObjetoEstrategia
                END", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool insereAssociacaoObjetivoProjeto(int codigoProjeto, int codigoObjetivo, char indicaObjetivoPrincipal, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoUpdate = "";

            if (indicaObjetivoPrincipal == 'S')
            {
                comandoUpdate = string.Format(@"UPDATE {0}.{1}.ProjetoObjetivoEstrategico SET IndicaObjetivoEstrategicoPrincipal = 'N'
                                                 WHERE CodigoProjeto =  @CodigoProjeto
                                                   AND IndicaObjetivoEstrategicoPrincipal = 'S'", bancodb, Ownerdb);
            }

            string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @CodigoProjeto int,
                            @CodigoObjetivo int,
                            @IndicaPrinciapal Char(1)
			
	                SET @CodigoProjeto = {2}
	                SET @CodigoObjetivo = {3}
	                SET @IndicaPrinciapal = '{4}'

                    {5}
                    
                    INSERT INTO {0}.{1}.ProjetoObjetivoEstrategico(CodigoProjeto, CodigoObjetivoEstrategico, IndicaObjetivoEstrategicoPrincipal, Prioridade)
                                                            VALUES(@CodigoProjeto, @CodigoObjetivo, @IndicaPrinciapal, 'A')
                    
                END              
                "
            , bancodb, Ownerdb, codigoProjeto, codigoObjetivo, indicaObjetivoPrincipal, comandoUpdate);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiAssociacaoObjetivoProjeto(int codigoProjeto, int codigoObjetivo, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                BEGIN
	                DECLARE @CodigoProjeto int,
                            @CodigoObjetivo int
			
	                SET @CodigoProjeto = {2}
	                SET @CodigoObjetivo = {3}
                    
                    DELETE FROM {0}.{1}.ProjetoObjetivoEstrategico
                     WHERE CodigoProjeto = @CodigoProjeto
                       AND CodigoObjetivoEstrategico = @CodigoObjetivo
                    
                END              
                "
            , bancodb, Ownerdb, codigoProjeto, codigoObjetivo);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }


    public DataSet getParceirosDisponiveisProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoProjeto int
			
	                SET @CodigoProjeto = {2}
	                
                    SELECT CodigoUsuario, NomeUsuario FROM {0}.{1}.f_tfp_GetPossiveisParceirosProjeto(@CodigoProjeto)
                    
                END", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }
    public DataSet getParceirosAssociadosProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoProjeto int
			
	                SET @CodigoProjeto = {2}
	                
                    SELECT CodigoUsuario, NomeUsuario FROM {0}.{1}.f_tfp_GetParceirosProjeto(@CodigoProjeto)
                    
                END", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool insereAssociacaoParceiroProjeto(int codigoProjeto, int codigoParceiro, int codigoUsuarioInclusao, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"EXEC {0}.{1}.p_tfp_adicionaParceiroProjeto  {2}, {3}, {4}              
                "
            , bancodb, Ownerdb, codigoProjeto, codigoParceiro, codigoUsuarioInclusao);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiAssociacaoParceiroProjeto(int codigoProjeto, int codigoParceiro, int codigoUsuarioInclusao, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"EXEC {0}.{1}.p_tfp_retiraParceiroProjeto  {2}, {3}, {4}              
                "
            , bancodb, Ownerdb, codigoProjeto, codigoParceiro, codigoUsuarioInclusao);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public DataSet getEtapasProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoProjeto int
			
	                SET @CodigoProjeto = {2}
	                
                    SELECT CodigoTarefa, SequenciaTarefaCronograma AS Sequencia, NomeTarefa,Inicio, Termino, PercentualFisicoConcluido
	                      ,CodigoUsuarioResponsavel AS CodigoResponsavel, NomeUsuarioResponsavel AS NomeResponsavel
                      FROM {0}.{1}.f_crono_GetTarefasProjeto(@CodigoProjeto)
                     ORDER BY SequenciaTarefaCronograma
                    
                END", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public bool insereTarefaProjeto(int codigoProjeto, int sequencia, string nomeTarefa, DateTime dataInicio, DateTime dataTermino, int percentConcluido, int codigoResponsavel, int codigoUsuarioInclusao, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
               BEGIN
	                DECLARE @DataInicio DateTime,
			                @DataTermino DateTime
			
	                SET @DataInicio = CONVERT(DateTime, '{5}', 103)
	                SET @DataTermino = CONVERT(DateTime, '{6}', 103)
		
	                EXEC {0}.{1}.p_crono_InsereTarefaCronograma  {2}, {3}, '{4}', @DataInicio, @DataTermino, {7}, {8}, {9}         
                END
                            
                "
            , bancodb, Ownerdb, codigoProjeto, sequencia, nomeTarefa, dataInicio, dataTermino, codigoResponsavel, codigoUsuarioInclusao, percentConcluido);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaTarefaProjeto(int codigoProjeto, int codigoTarefa, int sequencia, string nomeTarefa, DateTime dataInicio, DateTime dataTermino, int percentConcluido, int codigoResponsavel, int codigoUsuarioAlteracao, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                 BEGIN
	                DECLARE @DataInicio DateTime,
			                @DataTermino DateTime
			
	                SET @DataInicio = CONVERT(DateTime, '{6}', 103)
	                SET @DataTermino = CONVERT(DateTime, '{7}', 103)
		
	                EXEC {0}.{1}.p_crono_AlteraTarefaCronograma  {2}, {3}, {4}, '{5}', @DataInicio, @DataTermino, {8}, {9}, {10}
                END            
                "
            , bancodb, Ownerdb, codigoProjeto, codigoTarefa, sequencia, nomeTarefa, dataInicio, dataTermino, codigoResponsavel, codigoUsuarioAlteracao, percentConcluido);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiTarefaProjeto(int codigoProjeto, int codigoTarefa, int codigoUsuarioExclusao, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"EXEC {0}.{1}.p_crono_ExcluiTarefaCronograma  {2}, {3}, {4}              
                "
            , bancodb, Ownerdb, codigoProjeto, codigoTarefa, codigoUsuarioExclusao);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public DataSet getPendenciasProjeto(int codigoProjeto, string where)
    {
        DataSet dsRetorno = new DataSet();
        try
        {
            string comandoSQL = string.Format(
                 @"SELECT ISNULL(AtrasosCronograma, 0) AS AtrasosCronograma, ISNULL(AtrasosToDoList, 0) AS AtrasosToDoList, 
                      ISNULL(MensagensNaoRespondidas, 0) AS MensagensNaoRespondidas, ISNULL(QuestoesEmAberto, 0) AS QuestoesEmAberto,   
                      ISNULL(EntregasAtrasadas, 0) AS EntregasAtrasadas,  
                      ISNULL(ProdutosAtrasados, 0) AS ProdutosAtrasados,               
                      (SELECT p.DataUltimaAtualizacao FROM {0}.{1}.Projeto p
				        WHERE p.CodigoProjeto = {2}) AS DataUltimaAtualizacao,
                      (SELECT Convert(char(10), p.DataUltimaAtualizacao, 103) FROM {0}.{1}.Projeto p
				        WHERE p.CodigoProjeto = {2}) AS UltimaAtualizacao,
                      TerminoPrimeiraLB as TerminoPrimeiraLB
                 FROM {0}.{1}.f_GetNumerosPendenciaProjeto({2})
                  WHERE 1 = 1
                    {3}", bancodb, Ownerdb, codigoProjeto, where);
            dsRetorno = getDataSet(comandoSQL);
        }
        catch
        { }

        return dsRetorno;
    }

    public DataSet getNumerosDesempenhoProjeto(int codigoProjeto, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
             @"SELECT ISNULL(Sum(rp.TrabalhoLBData), 0) AS TotalTrabalhoPrevisto,
                     ISNULL(Sum(rp.TrabalhoLBTotal), 0) AS TotalTrabalhoPrevistoGeral,
                     ISNULL(Sum(rp.TrabalhoRealTotal), 0) AS TotalTrabalhoReal,
                     ISNULL(Sum(rp.CustoLBData{4}), 0) AS TotalCustoOrcado,
                     ISNULL(Sum(rp.CustoLBTotal{4}), 0) AS TotalCustoOrcadoGeral,
                     ISNULL(Sum(rp.CustoRealTotal{4}), 0) AS TotalCustoReal,
                     ISNULL(Sum(rp.ReceitaLBData{4}), 0) AS TotalReceitaOrcada,
                     ISNULL(Sum(rp.ReceitaLBTotal{4}), 0) AS TotalReceitaOrcadaGeral,
                     ISNULL(Sum(rp.ReceitaRealData{4}), 0) AS TotalReceitaReal
                FROM {0}.{1}.ResumoProjeto AS rp
	                 INNER JOIN {0}.{1}.Projeto AS p ON p.CodigoProjeto = rp.CodigoProjeto            
               WHERE p.CodigoProjeto = {2} {3} 
               ", bancodb, Ownerdb, codigoProjeto, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    public DataSet getCusteioInvestimentoProjeto(int codigoProjeto, int ano, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT DescricaoGrupoConta, Sum(ValorOrcado) AS ValorPrevisto, Sum(ValorReal) AS ValorReal 
                 FROM {0}.{1}.f_GetOrcamentoProjeto({2}, {3})
                WHERE 1 = 1
                  {4} 
                GROUP BY DescricaoGrupoConta
               ", bancodb, Ownerdb, codigoProjeto, ano, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getQuantidadeRiscosProblemasProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
              @"SELECT ISNULL(SUM(CASE WHEN CodigoStatusRiscoQuestao = 'RA' THEN 1 ELSE 0 END), 0) RiscosAtivos
	                  ,ISNULL(SUM(CASE WHEN CodigoStatusRiscoQuestao = 'RE' THEN 1 ELSE 0 END), 0) RiscosEliminados
	                  ,ISNULL(SUM(CASE WHEN CodigoStatusRiscoQuestao = 'QA' THEN 1 ELSE 0 END), 0) ProblemasAtivos 
	                  ,ISNULL(SUM(CASE WHEN CodigoStatusRiscoQuestao = 'QR' THEN 1 ELSE 0 END), 0) ProblemasEliminados 
                  FROM RiscoQuestao rq
                 WHERE rq.DataExclusao IS NULL
                   AND rq.DataPublicacao IS NOT NULL
                   AND ( rq.CodigoProjeto in 
                                            (
                                            select codigoprojetofilho from LinkProjeto LP inner join 
                                                   Projeto p on p.CodigoProjeto = lp.codigoprojetofilho and p.CodigoStatusProjeto = 3
                                            where LP.CodigoProjetoPai = {2}
                                            and LP.TipoLink = 'PP'
                                            )
                                            or rq.CodigoProjeto = {2} ) 
                   {3} 
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCoresBulletsProjeto(int codigoProjeto, int ano, string where)
    {
        string colunaNoAno = "";

        if (ano != -1)
            colunaNoAno = "NoAno";

        string comandoSQL = string.Format(
             @"SELECT dbo.f_GetCorTrabalho({2}) AS CorFisico
		             ,dbo.f_GetCorFinanceiro{4}({2}) AS CorFinanceiro
		             ,dbo.f_GetCorReceita{4}({2}) AS CorReceita
            ", bancodb, Ownerdb, codigoProjeto, where, colunaNoAno);
        return getDataSet(comandoSQL);
    }

    public void defineAlturaFrames(Page pagina, int alturaFrame)
    {
        if (pagina.Request.QueryString["FRM"] != null && pagina.Request.QueryString["FRM"].ToString() != "")
        {

            string script = string.Format(@"
                         <script language=""Javascript"" type=""text/javascript"">
                           parent.document.getElementById(""{0}"").height = {1}
                         </script>", pagina.Request.QueryString["FRM"].ToString()
                                   , alturaFrame);
            pagina.ClientScript.RegisterClientScriptBlock(pagina.GetType(), "client", script);
        }
    }

    public DataSet getDesempenhoFisicoProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT PercentualRealizacao * 100 AS PercentualReal, PercentualPrevistoRealizacao AS PercentualPrevisto, 
                      {0}.{1}.f_GetIDPAtual({2}) * 100 AS IDP, {0}.{1}.f_GetIDCAtual({2}) * 100 AS IDC, -1 AS PercentualPrevistoIDP, -1 AS PercentualPrevistoIDC,
                       {0}.{1}.f_GetCorFisico({2}) AS corDesempenho, {0}.{1}.f_crono_getPercentualPrevistoProjetoPrimeiraLB({2}, YEAR(GETDATE()), MONTH(GETDATE())) AS PercentualPrevistoLB1
                                   FROM {0}.{1}.ResumoProjeto
                                   WHERE CodigoProjeto = {2}
                                     {3}
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoCurvaSObjeto(int codigoEntidade, int codigoUsuario, string codigoProjeto, string codigoUnidade, string iniciais)
    {
        string comandoSQL = string.Format(
             @"SELECT Periodo, ValorPrevisto * 100 AS ValorPrevisto, ValorReal * 100 AS ValorReal
                    , ValorPrevistoLB1 * 100 AS ValorPrevistoLB1, ValorPrevistoTendencia * 100 AS ValorPrevistoTendencia, IndicaValorRealProjetado
	             FROM {0}.{1}.f_GetCurvaSFisicaGrupoProjeto({2}, {3}, {4}, '{5}', {6})
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoProjeto, iniciais, codigoUnidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoCurvaSFinanceiraObjeto(int codigoEntidade, int codigoUsuario, string codigoProjeto, string codigoUnidade, string iniciais)
    {
        string comandoSQL = string.Format(
             @"SELECT Periodo, ValorPrevisto * 100 AS ValorPrevisto, ValorReal * 100 AS ValorReal, ValorPrevistoPeriodo, ValorRealPeriodo, ValorPrevistoAcumulado, ValorRealAcumulado
	             FROM {0}.{1}.f_GetCurvaSFinanceiraGrupoProjeto({2}, {3}, {4}, '{5}', {6})
               ", bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoProjeto, iniciais, codigoUnidade);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoGeralObra(int codigoProjeto, int codigoEntidade, string indicaObra, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                      DECLARE @percTempoDecorrido numeric(10,4),
                              @percFisicoConcluido numeric(10,4),
                              @percMedido numeric(10, 4),
                              @CodigoProjeto int,
                              @CodigoContrato int,
                              @ValorPrevisto Decimal(25,4),                              
                              @ValorReal       Decimal(25,4),
                              @CodigoEntidade int,
                              @UtilizaCalculoSaldoPorValorPago Char(1),
                              @IndicaObra Char(1) --> Novo Parâmetro!!!!
                  
                      SET @CodigoProjeto = {2} 
                      SET @CodigoEntidade = {3}
                      SET @IndicaObra = '{4}'
            
                  SELECT @UtilizaCalculoSaldoPorValorPago = Valor
                    FROM {0}.{1}.ParametroConfiguracaoSistema
                   WHERE Parametro = 'calculaSaldoContratualPorValorContrato'
                     AND CodigoEntidade = @CodigoEntidade
            
                       IF @IndicaObra = 'S'
                          BEGIN
                            SELECT @CodigoContrato = ISNULL(c.CodigoContrato, -1),
                                   @ValorPrevisto = ISNULL(c.ValorContrato, 0)
                                FROM {0}.{1}.Projeto p INNER JOIN 
                                     {0}.{1}.Obra o ON o.CodigoProjeto = p.CodigoProjeto INNER JOIN
                                     {0}.{1}.Contrato c ON c.CodigoContrato = o.CodigoContrato
                                WHERE p.CodigoProjeto = @CodigoProjeto                                       
                                                                            
                                SELECT @percMedido = CASE WHEN @ValorPrevisto = 0 THEN 0
                                        ELSE(CASE WHEN ISNULL(@UtilizaCalculoSaldoPorValorPago, 'N') = 'S' THEN SUM(pc.ValorPago) ELSE SUM(pc.ValorPrevisto) END)/@ValorPrevisto END
                                 FROM {0}.{1}.ParcelaContrato pc 
                                WHERE pc.CodigoContrato = @CodigoContrato
                        END         
                        ELSE
                            BEGIN
                            SELECT @ValorPrevisto = Sum(f.ValorOrcado),
                                   @ValorReal = Sum(f.ValorReal)
                             FROM {0}.{1}.f_GetOrcamentoProjeto(@CodigoProjeto, YEAR(GETDATE())) AS f
                            WHERE f.DespesaReceita = 'D'
                                                            
                            IF @ValorPrevisto = 0 
                                SET @percMedido = 0
                            ELSE 
                                SET @percMedido = Convert(Decimal(10,4),@ValorReal/@ValorPrevisto)
                            END           
                  
                  SELECT @percTempoDecorrido = CASE WHEN DATEDIFF(DAY, TerminoLB, InicioLB) = 0 THEN 0
                                             ELSE CAST(DATEDIFF(DAY, GETDATE(), InicioLB) AS Decimal) /CAST(DATEDIFF(DAY, TerminoLB, InicioLB) AS Decimal) END
                    FROM {0}.{1}.vi_Projeto
                   WHERE CodigoProjeto = @CodigoProjeto          
                                                           
                      SELECT @percFisicoConcluido = rp.PercentualRealizacao 
                        FROM {0}.{1}.ResumoProjeto rp
                       WHERE rp.CodigoProjeto = @CodigoProjeto 
       
                       SELECT ISNULL(@percTempoDecorrido, 0) * 100 AS PercTempoDecorrido, 
                                             ISNULL(@percFisicoConcluido, 0) * 100 AS PercFisicoConcluido,
                                             ISNULL(@percMedido, 0) * 100 AS PercValorMedido
      
                END

               ", bancodb, Ownerdb, codigoProjeto, codigoEntidade, indicaObra, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getCurvaSPrevisaoProjeto(int codigoProjeto, int codigoPrevisao, char periodicidade, string where)
    {
        string comandoSQL = string.Format(
             @"EXEC {0}.{1}.p_curva_S_PrevisaoOrcamentaria  {2}, {3}, '{4}'
               ", bancodb, Ownerdb, codigoProjeto, codigoPrevisao, periodicidade, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadorDesempenhoFisico(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ValorInicial, ValorFinal 
                 FROM {0}.{1}.ParametroIndicadores
                WHERE TipoIndicador = 'FIS' {2}
                ORDER BY CodigoTipoStatus
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadorDesempenhoFinanceiro(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ValorInicial, ValorFinal 
                 FROM {0}.{1}.ParametroIndicadores
                WHERE TipoIndicador = 'FIN' {2}
                ORDER BY CodigoTipoStatus
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadorIDP(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ValorInicial, ValorFinal 
                 FROM {0}.{1}.ParametroIndicadores
                WHERE TipoIndicador = 'IDP' {2}
                ORDER BY CodigoTipoStatus
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getIndicadorIDC(string where)
    {
        string comandoSQL = string.Format(
             @"SELECT ValorInicial, ValorFinal 
                 FROM {0}.{1}.ParametroIndicadores
                WHERE TipoIndicador = 'IDC' {2}
                ORDER BY CodigoTipoStatus
               ", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool possuiAlertaCronograma(int codigoProjeto, ref string corIcone, ref string mensagem)
    {
        bool retorno = false;

        string comandoSQL = string.Format(
             @"BEGIN
	                DECLARE @CodigoProjeto int,
					        @TarefasSemLB int,
					        @RecalculaCrono Char(1)
	
	
	                SET @CodigoProjeto = {2}
		            SET @RecalculaCrono  = 'N';
		            SET @TarefasSemLB 	 = 0;
		            IF EXISTS (SELECT 1 FROM  {0}.{1}.CronogramaProjeto AS cp WHERE cp.CodigoProjeto = @CodigoProjeto AND cp.DataUltimaPublicacao IS NOT NULL)
		            BEGIN
	                    SELECT @RecalculaCrono = ISNULL(RecalcularCronogramaAoAbrirTasques, 'N')
		                  FROM {0}.{1}.CronogramaProjeto AS cp
	                    WHERE cp.CodigoProjeto = @CodigoProjeto

	                    -- verifica se há a situação de alerta LB no cronograma                        
	                    IF ( EXISTS 
                                ( SELECT TOP 1 1
                                  FROM {0}.{1}.[CronogramaProjeto] AS [cp]
                                    INNER JOIN {0}.{1}.[TarefaCronogramaProjeto] AS [tc] ON 
                                        (     tc.[CodigoCronogramaProjeto]  = cp.[CodigoCronogramaProjeto]
                                          AND tc.[DataExclusao]             IS NULL
                                          AND tc.[IndicaTarefaResumo]       = 'S'
                                          AND tc.[IndicaTarefaAtrasadaLB]   = 'S' )
                                  WHERE
                                        cp.[CodigoProjeto]  = @CodigoProjeto
                                    AND cp.[DataExclusao]   IS NULL
                                    AND NOT EXISTS 
                                        ( SELECT TOP 1 1 
                                            FROM {0}.{1}.[TarefaCronogramaProjeto] AS [tc2]
                                            WHERE tc2.[CodigoCronogramaProjeto] = tc.[CodigoCronogramaProjeto]
                                              AND tc2.[CodigoTarefaSuperior]    = tc.[CodigoTarefa]
                                              AND tc2.[DataExclusao]            IS NULL
                                              AND tc2.[IndicaTarefaAtrasadaLB]  = 'S'
                                        )
                                    AND EXISTS 
                                        ( SELECT TOP 1 1 
                                            FROM {0}.{1}.[TarefaCronogramaProjeto] AS [tc3]
                                            WHERE tc3.[CodigoCronogramaProjeto] = tc.[CodigoCronogramaProjeto]
                                              AND tc3.[CodigoTarefaSuperior]    = tc.[CodigoTarefa]
                                              AND tc3.[DataExclusao]            IS NULL
                                              AND tc3.[InicioLB]                IS NULL
                                        )
                                )
                           )
                        BEGIN
                            SET @TarefasSemLB = 1;
                        END -- IF EXISTS( 
                    END	-- IF EXISTS (SELECT 1 FROM  CronogramaProjeto AS cp WHERE...																			
	                SELECT @RecalculaCrono AS RecalculaCronograma
				          ,@TarefasSemLB AS TarefasSemLB
	                                      
                END
               ", bancodb, Ownerdb, codigoProjeto);

        DataSet ds = getDataSet(comandoSQL);

        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            string recalculaCrono = ds.Tables[0].Rows[0]["RecalculaCronograma"] + "";
            int tarefasSemCronograma = ds.Tables[0].Rows[0]["TarefasSemLB"] + "" != "" ? int.Parse(ds.Tables[0].Rows[0]["TarefasSemLB"] + "") : 0;

            if (recalculaCrono == "S" && tarefasSemCronograma > 0)
            {
                corIcone = "Vermelho";
                mensagem = @" - " + Resources.traducao.dados_h__pend_ncias_de_efetiva__o_de_tarefas_no_cronograma__para_resolv__las__utilize_a_op__o__editar_cronograma__ + "<br><br>- " + Resources.traducao.dados_tarefas_sem_linha_de_base_aprovada_est_o_gerando_indicativo_de_atraso_nas_tarefas_superiores_;
                retorno = true;
            }
            else if (recalculaCrono == "S")
            {
                corIcone = "Vermelho";
                mensagem = @" - " + Resources.traducao.dados_h__pend_ncias_de_efetiva__o_de_tarefas_no_cronograma__para_resolv__las__utilize_a_op__o__editar_cronograma__;
                retorno = true;
            }
            else if (tarefasSemCronograma > 0)
            {
                corIcone = "Amarelo";
                mensagem = @" - " + Resources.traducao.dados_tarefas_sem_linha_de_base_aprovada_est_o_gerando_indicativo_de_atraso_nas_tarefas_superiores_;
                retorno = true;
            }
        }

        return retorno;
    }

    public string getGraficoDesempenhoFisico(DataTable dt, DataTable dtIndicadores, int fonte, string urlImage, string colPercPrevisto, string colPercReal, string titulo, bool mostraCorLaranja)
    {
        StringBuilder xml = new StringBuilder();

        float percentualFisico = 0;
        float percentualPrevisto = 0;
        string real = "";
        string previsto = "";
        string desempenhoProjeto = "";
        string corNivel = corSatisfatorio;

        try
        {
            desempenhoProjeto = mostraCorLaranja ? dt.Rows[0]["corDesempenho"].ToString() : "";
            real = dt.Rows[0][colPercReal].ToString();
            previsto = dt.Rows[0][colPercPrevisto].ToString();
            if ((real != "") && (previsto != ""))
            {

                percentualFisico = float.Parse(real);
                percentualPrevisto = float.Parse(previsto);

            }
        }
        catch
        {

        }

        string exportar = "";

        if (desempenhoProjeto == "Laranja" && mostraLaranja == "S")
            corNivel = "FF6600";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1"" ";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }

        float valorPercentualPrevisto = percentualPrevisto;

        percentualPrevisto = percentualPrevisto == -1 ? 1 : percentualPrevisto;



        //configuração geral do gráfico
        xml.Append("<chart imageSave=\"1\" imageSaveURL=\"" + urlImage + "\" upperLimit=\"100\" showValue=\"1\" decimals=\"2\" decimalSeparator=\".\" " +
                            "inDecimalSeparator=\",\" baseFontSize=\"" + fonte + "\"  " + exportar +
                            "bgColor=\"FFFFFF\" showBorder=\"0\" chartTopMargin=\"5\" " +
                            "chartBottomMargin=\"20\" gaugeFillMix=\"\" lowerLimit=\"0\" numberSuffix=\"%\" " +
                            "gaugeFillRatio=\"\" >");
        xml.Append(" <colorRange>");

        if (percentualFisico == 100)
        {
            try
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                    "\" maxValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                xml.Append(" <color minValue=\"" + (int)(float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            catch { }
        }
        else
        {
            if ((percentualPrevisto * 100) == 1)
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"1\" name=\" \" code=\"" + corCritico + "\" />");
                xml.Append(" <color minValue=\"1\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            else if ((percentualPrevisto * 100) <= 1 && valorPercentualPrevisto > 0)
            {
                //define a escala do gráfico
                xml.Append(" <color minValue=\"0\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
            }
            else
            {
                if ((percentualPrevisto * 100) <= 2 && valorPercentualPrevisto > 0)
                {
                    //define a escala do gráfico
                    xml.Append(" <color minValue=\"0\" maxValue=\"1\" name=\" \" code=\"" + corCritico + "\" />");
                    xml.Append(" <color minValue=\"1\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                }
                else
                {
                    if ((percentualPrevisto * 100) == 100 && valorPercentualPrevisto > 0 && (desempenhoProjeto != "Laranja" || mostraLaranja == "N"))
                    {
                        //define a escala do gráfico
                        xml.Append(" <color minValue=\"0\" maxValue=\"99\" name=\" \" code=\"" + corCritico + "\" />");
                        xml.Append(" <color minValue=\"99\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                    }
                    else
                    {
                        try
                        {
                            //define a escala do gráfico
                            xml.Append(" <color minValue=\"0\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) + "\" name=\" \" code=\"" + corCritico + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[0][1].ToString())) +
                                "\" maxValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" name=\" \" code=\"" + corAtencao + "\" />");
                            xml.Append(" <color minValue=\"" + (int)(percentualPrevisto * float.Parse(dtIndicadores.Rows[1][1].ToString())) + "\" maxValue=\"100\" name=\" \" code=\"" + corNivel + "\" />");
                        }
                        catch { }
                    }
                }
            }
        }

        string percentual = "" + percentualPrevisto * 100;

        xml.Append(" </colorRange>");
        xml.Append(" <dials>");
        xml.Append(" <dial value=\"" + (percentualFisico) + "\"/>");
        xml.Append(" </dials>");
        xml.Append(" <trendpoints>");
        xml.Append(" <point startValue=\"" + percentual + "\" markerTooltext=\"" + Resources.traducao.meta + " = " + percentual + "%\" useMarker=\"1\" dashed=\"1\" dashLen=\"2\" dashGap=\"2\" valueInside=\"1\"/>");
        xml.Append(" </trendpoints>");
        xml.Append("</chart>");

        return xml.ToString();
    }

    public string getGraficoDesempenhoGeralObra(DataTable dt, string titulo, int fonte, bool mostrarLink, int codigoProjeto, bool indicaObra)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();

        int i = 0;
        string link = "";

        string tituloValorMedido = indicaObra ? "% Valor Medido" : "% Orçamento";

        try
        {
            xmlAux.Append(string.Format(@"<set label=""% Tempo Decorrido{1}"" value=""{0}"" color=""3399FF"" />", double.Parse(dt.Rows[i]["PercTempoDecorrido"].ToString()) > 100 ? "100" : dt.Rows[i]["PercTempoDecorrido"].ToString(),
                indicaObra ? "{br}(Obra)" : ""
                ));

            xmlAux.Append(string.Format(@"<set label=""% Físico Concluído"" value=""{0}"" color=""3399FF"" />", dt.Rows[i]["PercFisicoConcluido"].ToString()));

            if (mostrarLink)
                link = string.Format(@"link=""JavaScript: isJavaScriptCall=true; abreDesempenhoFinanceiro({0});"" ", codigoProjeto);

            xmlAux.Append(string.Format(@"<set label=""{2}"" value=""{0}"" color=""3399FF"" {1} />", dt.Rows[i]["PercValorMedido"].ToString(), link, tituloValorMedido));
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

        xml.Append(string.Format(@"<chart caption=""{0}"" showPlotBorder=""0"" canvasBgColor=""F7F7F7"" showBorder=""0"" BgColor=""F7F7F7"" yAxisMinValue=""0"" yAxisMaxValue=""100"" inDecimalSeparator="","" decimalSeparator="",""
             chartTopMargin=""5"" chartRightMargin=""8"" inThousandSeparator=""."" chartBottomMargin=""2"" chartLeftMargin=""4"" numberSuffix=""%""
             adjustDiv=""1""  slantLabels=""1"" labelDisplay=""ROTATE"" showLegend=""S"" minimiseWrappingInLegend=""1""  legendNumColumns=""3"" 
             showValues=""1"" {1} showShadow=""0"" showLabels=""1"" decimals=""2"" baseFontSize=""{2}"">", titulo
                                                                                                         , usarGradiente + usarBordasArredondadas + exportar
                                                                                                         , fonte));

        xml.Append(xmlAux.ToString());
        xml.Append("<styles>");
        xml.Append("<definition>");
        xml.Append(string.Format(@"<style name=""fontLabels"" type=""font"" face=""Verdana"" size=""{0}"" color=""000000"" bold=""0""/>", fonte));
        xml.Append(string.Format(@"<style name=""fontTitulo"" type=""font"" face=""Verdana"" size=""{0}"" color=""000000"" bold=""0""/>", (fonte + 2)));
        xml.Append(string.Format(@"<style name=""myBevel"" type=""bevel"" strength=""3"" distance=""5""/>"));
        xml.Append("</definition>");
        xml.Append("<application>");
        xml.Append(@"<apply toObject=""DATALABELS"" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""DATAVALUES"" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""XAXISNAME "" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""YAXISNAME"" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""YAXISVALUES"" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""TOOLTIP"" styles=""fontLabels"" />");
        xml.Append(@"<apply toObject=""Caption"" styles=""fontTitulo"" />");
        xml.Append(@"<apply toObject=""DATAPLOT"" styles=""myBevel"" />");
        xml.Append("</application>");
        xml.Append("</styles>");
        xml.Append("</chart>");


        return xml.ToString();
    }

    public string getGraficoDesempenhoRiscosQuestoes(DataTable dt, string titulo, int fonte, bool linkQuestoes, bool linkRiscos, int idProjeto)
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

            string link = "";

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Sob Controle\"  color=\"" + corSatisfatorio + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Tipo"].ToString() == "R" && linkRiscos == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=R&Cor=Verde&idProjeto={0}"" ", idProjeto);
                else if (dt.Rows[i]["Tipo"].ToString() == "Q" && linkQuestoes == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=Q&Cor=Verde&idProjeto={0}"" ", idProjeto);

                xmlAux.Append("<set label=\"Sob Controle\" value=\"" + dt.Rows[i]["Verdes"].ToString() + "\" color=\"" + corSatisfatorio + "\" " + link + "/>");
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Atenção\" color=\"" + corAtencao + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Tipo"].ToString() == "R" && linkRiscos == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=R&Cor=Amarelo&idProjeto={0}"" ", idProjeto);
                else if (dt.Rows[i]["Tipo"].ToString() == "Q" && linkQuestoes == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=Q&Cor=Amarelo&idProjeto={0}"" ", idProjeto);

                xmlAux.Append("<set label=\"Atenção\" value=\"" + dt.Rows[i]["Amarelos"].ToString() + "\"  color=\"" + corAtencao + "\" " + link + "/>");
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Críticos\" color=\"" + corCritico + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Tipo"].ToString() == "R" && linkRiscos == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=R&Cor=Vermelho&idProjeto={0}"" ", idProjeto);
                else if (dt.Rows[i]["Tipo"].ToString() == "Q" && linkQuestoes == true)
                    link = string.Format(@"link=""F-framePrincipal-../riscos.aspx?Publicado=SIM&TT=Q&Cor=Vermelho&idProjeto={0}"" ", idProjeto);

                xmlAux.Append("<set label=\"Críticos\" value=\"" + dt.Rows[i]["Vermelhos"].ToString() + "\" color=\"" + corCritico + "\" " + link + "/>");
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
                 " showShadow=\"0\" " + usarGradiente + usarBordasArredondadas + exportar + " showBorder=\"0\" baseFontSize=\"" + fonte + "\" decimalPrecision=\"0\" numDivLines=\"" + (int)(qtdProjetos) + "\">");
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

    public DataSet getGradeRiscosQuestoes(int codigoProjeto, char riscoQuestao, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoProjeto Int,
                          @RiscoQuestao Char(1)  

                  SET @CodigoProjeto = {2}
                  SET @RiscoQuestao = '{3}'  

                  SELECT CAST( CASE trq.Polaridade WHEN 'NEG' THEN rq.ProbabilidadePrioridade ELSE (4-rq.ProbabilidadePrioridade) END AS TinyInt ) AS Coluna,
                         CAST( CASE trq.Polaridade WHEN 'NEG' THEN rq.ImpactoUrgencia ELSE (4-rq.ImpactoUrgencia) END AS TinyInt ) AS Linha,
                         COUNT(1) AS Quantidade
                    FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN {0}.{1}.TipoRiscoQuestao AS trq ON (trq.CodigoTipoRiscoQuestao = rq.CodigoTipoRiscoQuestao)
                   WHERE rq.CodigoProjeto = @CodigoProjeto
                     AND rq.IndicaRiscoQuestao = @RiscoQuestao
                     AND rq.DataPublicacao IS NOT NULL
                     AND rq.CodigoStatusRiscoQuestao IN ('QA','RA')
                     AND rq.DataExclusao IS NULL 
                  GROUP BY CAST( CASE trq.Polaridade WHEN 'NEG' THEN rq.ProbabilidadePrioridade ELSE (4-rq.ProbabilidadePrioridade) END AS TinyInt ),
                         CAST( CASE trq.Polaridade WHEN 'NEG' THEN rq.ImpactoUrgencia ELSE (4-rq.ImpactoUrgencia) END AS TinyInt )
                END", bancodb, Ownerdb, codigoProjeto, riscoQuestao, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDesempenhoRiscosQuestoesProjeto(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"BEGIN
                  DECLARE @CodigoProjeto Int,
                          @RiscoQuestao Char(1),
                          @labelQuestoes VarChar(15)
                  
                  DECLARE @tbResumo AS Table(Descricao VarChar(10),
							                 Verdes int,
							                 Amarelos int,
							                 Vermelhos int,
                                             Tipo Char(1))
                  
                  SET @CodigoProjeto = {2}

                  SELECT @labelQuestoes = Valor 
                    FROM {0}.{1}.ParametroConfiguracaoSistema 
                   WHERE Parametro = 'labelQuestoes'
                     AND CodigoEntidade = (SELECT CodigoEntidade 
                                             FROM {0}.{1}.Projeto 
                                            WHERE CodigoProjeto = @CodigoProjeto)
                  
                /* Riscos Negativos */  
                INSERT INTO @tbResumo
                  SELECT 'Riscos', 
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Verde' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Amarelo' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END), 0),
                         'R'
                    FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN {0}.{1}.TipoRiscoQuestao	AS trq ON (trq.CodigoTipoRiscoQuestao = rq.CodigoTipoRiscoQuestao)
                   WHERE rq.CodigoProjeto = @CodigoProjeto
                     AND rq.IndicaRiscoQuestao = 'R'
                     AND rq.DataPublicacao IS NOT NULL
                     AND rq.CodigoStatusRiscoQuestao = 'RA'  
                     AND rq.DataExclusao IS NULL 
                     AND trq.Polaridade = 'NEG'
                
                 /* Riscos Positivos */ 
                 INSERT INTO @tbResumo  
                 SELECT 'Riscos', 
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Verde' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Amarelo' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(rq.CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END), 0),
                         'R'
                    FROM {0}.{1}.RiscoQuestao AS rq INNER JOIN {0}.{1}.TipoRiscoQuestao	AS trq ON (trq.CodigoTipoRiscoQuestao = rq.CodigoTipoRiscoQuestao)
                   WHERE rq.CodigoProjeto = @CodigoProjeto
                     AND rq.IndicaRiscoQuestao = 'R'
                     AND rq.DataPublicacao IS NOT NULL
                     AND rq.CodigoStatusRiscoQuestao = 'RA' 
                     AND rq.DataExclusao IS NULL 
                     AND trq.Polaridade = 'POS'
                     
                 INSERT INTO @tbResumo
                  SELECT ISNULL(@labelQuestoes, 'Questões'), 
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(CodigoRiscoQuestao) = 'Verde' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(CodigoRiscoQuestao) = 'Amarelo' THEN 1 ELSE 0 END), 0),
		                 ISNULL(SUM(CASE WHEN {0}.{1}.f_GetCorRiscoQuestao(CodigoRiscoQuestao) = 'Vermelho' THEN 1 ELSE 0 END), 0),
                         'Q'
                    FROM {0}.{1}.RiscoQuestao
                   WHERE CodigoProjeto = @CodigoProjeto
                     AND IndicaRiscoQuestao = 'Q'
                     AND DataPublicacao IS NOT NULL
                     AND CodigoStatusRiscoQuestao IN ('QA','RA')
                     AND DataExclusao IS NULL 
                     
                     SELECT Descricao,
                            SUM(Verdes) AS Verdes,
                            SUM(Amarelos) AS Amarelos,
                            SUM(Vermelhos) AS Vermelhos,
                            Tipo
                       FROM @tbResumo       
                      GROUP BY Descricao, Tipo
                     
                END", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDadosRecursosHumanos(int idProjeto)
    {
        string comandoSQL = string.Format(
        @"SELECT YEAR(Data) AS Ano,
            CASE  MONTH(Data)
                WHEN 1 then 'Janeiro'
                WHEN 2 then 'Fevereiro'
                WHEN 3 then 'Março'
                WHEN 4 then 'Abril'
                WHEN 5 then 'Maio'
                WHEN 6 then 'Junho'
                WHEN 7 then 'Julho'
                WHEN 8 then 'Agosto'
                WHEN 9 then 'Setembro'
                WHEN 10 then 'Outubro'
                WHEN 11 then 'Novembro'
                WHEN 12 then 'Dezembro'
             END as Mes,
            Data,
            vi.NomeRecurso,
            vi.NomeTarefa,
            SUM(vi.Trabalho) AS Trabalho,
            SUM(vi.TrabalhoReal) AS TrabalhoReal,
            SUM(vi.TrabalhoLB) AS TrabalhoLB,
            SUM(Vi.TrabalhoHE) AS TrabalhoHE,
            SUM(vi.Trabalho - vi.TrabalhoReal) AS TrabalhoRestante,
            SUM(vi.Trabalho - vi.TrabalhoLB) AS VariacaoTrabalho,
            SUM(vi.Custo) AS Custo,
            SUM(vi.CustoReal) AS CustoReal,
            SUM(vi.CustoLB) AS CustoLB,
            SUM(Vi.CustoHE) AS CustoHE,
            SUM(vi.Custo - vi.CustoReal) AS CustoRestante,
            SUM(vi.Custo - vi.CustoLB) AS VariacaoCusto
        FROM {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi 
        WHERE vi.CodigoProjeto IN (SELECT CodigoProjetoFilho 
                                   FROM LinkProjeto
                                  WHERE CodigoProjetoPai = {2}
                                  UNION
                                  SELECT {2})
               AND vi.NomeRecurso IS NOT NULL
               AND vi.TipoRecurso = 1
         GROUP BY YEAR(Data),
                 MONTH(Data),
                 Data,
                 vi.NomeRecurso,
                vi.NomeTarefa", bancodb, Ownerdb, idProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getRecursosHumanosProjeto(int idProjeto)
    {
        string comandoSQL = string.Format(
        @"SELECT vi.NomeRecurso,
            SUM(vi.Trabalho) AS Trabalho,
            SUM(vi.TrabalhoReal) AS TrabalhoReal,
            SUM(vi.TrabalhoLB) AS TrabalhoLB,
            SUM(Vi.TrabalhoHE) AS TrabalhoHE,
            SUM(vi.Trabalho - vi.TrabalhoReal) AS TrabalhoRestante,
            SUM(vi.Trabalho - vi.TrabalhoLB) AS VariacaoTrabalho,
            SUM(vi.Custo) AS Custo,
            SUM(vi.CustoReal) AS CustoReal,
            SUM(vi.CustoLB) AS CustoLB,
            SUM(Vi.CustoHE) AS CustoHE,
            SUM(vi.Custo - vi.CustoReal) AS CustoRestante,
            SUM(vi.Custo - vi.CustoLB) AS VariacaoCusto
        FROM {0}.{1}.vi_AtribuicaoDiariaRecurso AS vi 
         WHERE vi.CodigoProjeto IN (SELECT CodigoProjetoFilho 
                                  FROM {0}.{1}.LinkProjeto
                                 WHERE CodigoProjetoPai = {2}
                               UNION
                               SELECT {2})                               
          AND vi.NomeRecurso IS NOT NULL
          AND vi.TipoRecurso <> 3
        GROUP BY vi.NomeRecurso", bancodb, Ownerdb, idProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getRecursosHumanosProjetoCurvaS(int idProjeto, int codigoEntidade, int codigoUsuario)
    {
        string iniciaisTipoProjeto = getIniciaisTipoProjeto(idProjeto);

        if (iniciaisTipoProjeto.ToUpper() == "PRG")
        {
            iniciaisTipoProjeto = "PROG";
        }
        else
        {
            iniciaisTipoProjeto = "PROJ";
        }

        string comandoSQL = string.Format(
        @"SELECT Periodo, TrabalhoPrevisto * 100 AS PercentualPrevisto, TrabalhoReal * 100 AS PercentualReal
                ,TrabalhoPrevistoPeriodo AS PrevistoPeriodo, TrabalhoRealPeriodo AS RealPeriodo, TrabalhoPrevistoAcumulado AS PrevistoAcumulado, TrabalhoRealAcumulado AS RealAcumulado
            FROM {0}.{1}.f_GetCurvaSTrabalhoGrupoProjeto({2}, {3}, {4}, '{5}', null)", bancodb, Ownerdb, codigoEntidade, codigoUsuario, idProjeto, iniciaisTipoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getFinanceiroProjetoCurvaS(int idProjeto, int codigoEntidade, int codigoUsuario)
    {
        string iniciaisTipoProjeto = getIniciaisTipoProjeto(idProjeto);

        if (iniciaisTipoProjeto.ToUpper() == "PRG")
        {
            iniciaisTipoProjeto = "PROG";
        }
        else
        {
            iniciaisTipoProjeto = "PROJ";
        }

        string comandoSQL = string.Format(
        @"SELECT Periodo, ValorPrevisto * 100 AS PercentualPrevisto, ValorReal * 100 AS PercentualReal, Mes, Ano
                ,ValorPrevistoPeriodo AS PrevistoPeriodo, ValorRealPeriodo AS RealPeriodo, ValorPrevistoAcumulado AS PrevistoAcumulado, ValorRealAcumulado AS RealAcumulado
            FROM {0}.{1}.f_GetCurvaSFinanceiraGrupoProjeto({2}, {3}, {4}, '{5}', null)", bancodb, Ownerdb, codigoEntidade, codigoUsuario, idProjeto, iniciaisTipoProjeto);
        return getDataSet(comandoSQL);
    }

    public string getGraficoProjetoCurvaS(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""10"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""0""  numVisiblePlot=""{5}""  showBorder=""0"" BgColor=""F7F7F7"" labelDisplay=""ROTATE"" slantLabels=""1"" 
                                    inDecimalSeparator ="",""  showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
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

        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" showValues=""0"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualPrevisto"].ToString();
                string valorPeriodo = dt.Rows[i]["PrevistoPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["PrevistoAcumulado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));
                string displayValueAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorAcumulado));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Previsto: {1}%{2}Valor Previsto(No Período): {3}{2}Valor Previsto(Acumulado): {4}"" /> ", valor == "" ? "0" : valor, displayValue, "{BR}", displayValuePeriodo, displayValueAcumulado));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" showValues=""0"" >"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualReal"].ToString();
                string valorPeriodo = dt.Rows[i]["RealPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["RealAcumulado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));
                string displayValueAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorAcumulado));

                xml.Append(string.Format(@"<set value=""{0}"" toolText=""Realizado: {1}%{2}Valor Realizado(No Período): {3}{2}Valor Realizado(Acumulado): {4}"" dashed=""0""/> ", valor, displayValue, "{BR}", displayValuePeriodo, displayValueAcumulado));
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

    public string getGraficoProjetoCurvaSComLabel(DataTable dt, string titulo, int fonte, int casasDecimais, int scrollHeight, int numDivLines, int numVisiblePlot)
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

        xml.Append(string.Format(@"<chart palette=""2"" {0} caption=""{1}"" baseFontSize=""{2}"" chartBottomMargin=""2"" chartLeftMargin=""5"" chartRightMargin=""10"" chartTopMargin=""5"" canvasBgColor=""F7F7F7"" canvasLeftMargin=""15"" canvasRightMargin=""20"" canvasBottomMargin=""2""
                                    numDivLines=""{4}"" showvalues=""1""  numVisiblePlot=""{5}""  showBorder=""0"" BgColor=""F7F7F7"" labelDisplay=""ROTATE"" slantLabels=""1"" 
                                    inDecimalSeparator ="",""  showLegend=""1""  inThousandSeparator=""."" decimalSeparator="","" thousandSeparator=""."" >", usarGradiente + usarBordasArredondadas + exportar
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

        xml.Append(string.Format(@"<dataset seriesName=""Previsto"" showValues=""1"">"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualPrevisto"].ToString();
                string valorPeriodo = dt.Rows[i]["PrevistoPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["PrevistoAcumulado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));
                string displayValueAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorAcumulado));
                bool mostrarValores = dt.Rows[i]["Mes"].ToString() == DateTime.Now.Month.ToString() && dt.Rows[i]["Ano"].ToString() == DateTime.Now.Year.ToString();
                xml.Append(string.Format(@"<set value=""{0}"" displayValue=""{5}"" toolText=""Previsto: {1}%{2}Valor Previsto(No Período): {3}{2}Valor Previsto(Acumulado): {4}"" /> ", valorAcumulado == "" ? "0" : valorAcumulado, displayValue, "{BR}", displayValuePeriodo, displayValueAcumulado, mostrarValores ? displayValueAcumulado : " "));
            }
        }
        catch
        {
            return "";
        }

        xml.Append(string.Format(@"</dataset>"));

        xml.Append(string.Format(@"<dataset seriesName=""Realizado"" showValues=""1"" >"));

        try
        {
            for (i = 0; i < dt.Rows.Count; i++)
            {
                string valor = dt.Rows[i]["PercentualReal"].ToString();
                string valorPeriodo = dt.Rows[i]["RealPeriodo"].ToString();
                string valorAcumulado = dt.Rows[i]["RealAcumulado"].ToString();

                string displayValue = valor == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valor));
                string displayValuePeriodo = valorPeriodo == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorPeriodo));
                string displayValueAcumulado = valorAcumulado == "" ? "-" : string.Format("{0:n" + casasDecimais + "}", double.Parse(valorAcumulado));
                bool mostrarValores = dt.Rows[i]["Mes"].ToString() == DateTime.Now.Month.ToString() && dt.Rows[i]["Ano"].ToString() == DateTime.Now.Year.ToString();

                xml.Append(string.Format(@"<set value=""{0}"" displayValue=""{5}"" toolText=""Realizado: {1}%{2}Valor Realizado(No Período): {3}{2}Valor Realizado(Acumulado): {4}"" dashed=""0""/> ", valorAcumulado, displayValue, "{BR}", displayValuePeriodo, displayValueAcumulado, mostrarValores ? displayValueAcumulado : " "));
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


    public string getGraficoRHProjetoPizza(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }
        try
        {

            //configuração geral do gráfico
            xml.Append(string.Format(@"<chart caption=""{0}"" adjustDiv=""1""  showLegend=""0"" labelDistance=""1"" enablesmartlabels=""0""
                  decimalSeparator="","" showZeroPies=""0"" thousandSeparator=""."" inDecimalSeparator="","" inThousandSeparator="".""
                  BgColor=""F7F7F7"" canvasBgColor=""F7F7F7"" decimals=""2"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""8"" chartBottomMargin=""2"" chartLeftMargin=""4"" formatNumber=""1""
                  showShadow=""0"" {2} showBorder=""0"" baseFontSize=""{1}"" numberSuffix=""h"" >", titulo, fonte, usarGradiente + usarBordasArredondadas + exportar));

            int i = 0;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""{0}"" value=""{1}"" />", dt.Rows[i]["NomeRecurso"].ToString(), dt.Rows[i]["Trabalho"].ToString()));

            }
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

    public DataSet getTarefasProjeto(int codigoProjeto, string data, int codigoRecurso, string where, string colunaOrderBy)
    {
        if (codigoRecurso == -1)
            colunaOrderBy = "Inicio";

        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @DataParam DateTime,
			                @CodigoProjeto int,
                            @CodigoRecurso int
                	
	                SET @DataParam = CONVERT(DateTime, '{3}', 103)
	                SET @CodigoProjeto = {2}
                    SET @CodigoRecurso = {4}
                	
                    SELECT * FROM {0}.{1}.f_GetCronogramaProjeto(@CodigoProjeto, @DataParam, @CodigoRecurso)    
                     WHERE 1 = 1 
                       {5}
                    ORDER BY {6}
                END
               ", bancodb, Ownerdb, codigoProjeto, data, codigoRecurso, where, colunaOrderBy);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnotacoesTarefasProjeto(int codigoProjeto, string where)
    {

        string comandoSQL = string.Format(
             @"SELECT AnotacoesTarefa
                FROM {0}.{1}.vi_Tarefa AS t
               WHERE t.CodigoProjeto = {2}
                 {3}
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getGanttTarefasProjeto(int codigoProjeto, int codigoRecurso, string where)
    {

        if (codigoRecurso != -1)
        {
            where += string.Format(@" AND t.CodigoTarefa IN (SELECT a.CodigoTarefa 
                                                               FROM {0}.{1}.vi_Atribuicao a
                                                              WHERE a.CodigoProjeto = {2}
                                                                AND a.CodigoRecurso = {3})", bancodb, Ownerdb, codigoProjeto, codigoRecurso);
        }

        string comandoSQL = string.Format(
             @"BEGIN 
	                DECLARE @CodigoProjeto int
                	
	                SET @CodigoProjeto = {2}
                	
                    SELECT t.CodigoTarefa AS Codigo, t.NomeTarefa AS Descricao, 
                	       CONVERT(Char(10), t.Termino, 103) AS Termino, CONVERT(Char(10), t.TerminoReal, 103) AS TerminoReal
                           , CONVERT(Char(10), t.Inicio, 103) AS Inicio, CONVERT(Char(10), InicioReal, 103) AS InicioReal,
                	       CASE WHEN TerminoLB < GetDate() AND TerminoReal IS NULL THEN 'Vermelho'
                				WHEN TerminoReal IS NOT NULL THEN 'Cinza' ELSE 'Verde' END AS Cor,
                           t.IndicaMarco AS Marco, Nivel
                    FROM {0}.{1}.vi_Tarefa AS t
                   WHERE t.CodigoProjeto = @CodigoProjeto 
                     {3}
                   ORDER BY t.Inicio, t.Termino
                END
               ", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getDatasGanttTarefasProjetos(int codigoProjeto, int codigoRecurso, string where)
    {
        if (codigoRecurso != -1)
        {
            where += string.Format(@" AND t.CodigoTarefa IN (SELECT a.CodigoTarefa 
                                                               FROM {0}.{1}.vi_Atribuicao a
                                                              WHERE a.CodigoProjeto = {2}
                                                                AND a.CodigoRecurso = {3})", bancodb, Ownerdb, codigoProjeto, codigoRecurso);
        }

        string comandoSQL = string.Format(
               @"SELECT MIN(Inicio) AS Inicio, MAX(Termino) AS Termino, DATEDIFF(MONTH, MIN(Inicio), MAX(Termino)) AS Meses
		                   FROM {0}.{1}.vi_Tarefa AS t
               		  	  WHERE t.CodigoProjeto = {2} 
               		  	    {3}",
               bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getRecursosCronograma(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT DISTINCT a.NomeRecurso AS Recurso, a.CodigoRecurso
				FROM {0}.{1}.vi_Atribuicao a
                WHERE a.CodigoProjeto = {2}
                  AND a.TipoRecurso = 1    
                  {3}             
               ORDER BY a.NomeRecurso", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getNiveisCronograma(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(
             @"SELECT DISTINCT Nivel 
                 FROM {0}.{1}.vi_Tarefa 
                WHERE CodigoProjeto = {2} 
                  {3}   
               ORDER BY Nivel", bancodb, Ownerdb, codigoProjeto, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getRecursosTarefasProjeto(int codigoProjeto, string codigoTarefa, string where)
    {
        string comandoSQL = string.Format(
             @"	SELECT a.CodigoProjeto,
                       a.CodigoMSProject,
                       a.NomeRecurso AS NomeRecurso,
                       a.TipoRecurso AS TipoRecurso,
                       Sum(a.Custo) AS Custo,
                       Sum(a.CustoReal) AS CustoReal,
                       Sum(a.CustoLB) AS CustoPrevisto,
                       Sum(a.CustoHE) AS CustoHoraExtra,
                       Sum(a.CustoRealHE) AS CustoRealHoraExtra,
                       Sum(a.Trabalho) AS Trabalho,
                       Sum(a.TrabalhoReal) AS TrabalhoReal,
                       Sum(a.TrabalhoLB) AS TrabalhoPrevisto,
                       Sum(a.TrabalhoHE) AS TrabalhoHoraExtra,
                       Sum(a.TrabalhoRealHE) AS TrabalhoRealHoraExtra,
                       Sum(a.VariacaoTrabalho) AS VariacaoTrabalho,
                       Sum(a.VariacaoCusto) AS VariacaoCusto
                FROM {0}.{1}.vi_Atribuicao AS a 
               WHERE a.CodigoProjeto = {2}
                 {4}
                 AND a.CodigoTarefa IN (SELECT t.CodigoTarefa
                                          FROM {0}.{1}.vi_Tarefa AS t
                                         WHERE t.CodigoProjeto = a.CodigoProjeto
                                           AND t.EstruturaHierarquica LIKE (SELECT vi.EstruturaHierarquica
                                                                              FROM {0}.{1}.vi_Tarefa AS vi
                                                                             WHERE vi.CodigoTarefa = '{3}'
                                                                               AND vi.CodigoProjeto = a.CodigoProjeto) + '%')
            GROUP BY a.CodigoProjeto,
                     a.CodigoMSProject,
                     a.NomeRecurso,
                     a.TipoRecurso
               ", bancodb, Ownerdb, codigoProjeto, codigoTarefa, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMapaEntregaProjeto(string dataPrevistaInicial, string dataPrevistaFinal, int codigoUsuario, int codigoCarteira, int codigoEntidade)
    {
        string comandoSQL = string.Format(
        @"SELECT 
		         lp.CodigoProjetoPai as CodigoPrograma,
				ISNULL((select NomeProjeto from projeto where codigoprojeto = lp.codigoprojetopai), 'S/I') as NomePrograma,
                 p.NomeProjeto,
				 e.NomeTarefa AS Tarefa,
				 u.NomeUsuario AS GerenteProjeto,
				 he.DataPrevistaEntrega,
				 he.SequenciaHistoricoEntrega,
				 CASE WHEN he.SequenciaHistoricoEntrega = (SELECT Max(HistoricoEntrega.SequenciaHistoricoEntrega)
										                     FROM {0}.{1}.HistoricoEntrega
										                    WHERE HistoricoEntrega.CodigoEntrega = he.CodigoEntrega) THEN e.DataRealEntrega ELSE Null END AS DataRealEntrega,
				 t.Receita AS ValorFaturamento,
				 CONVERT(DateTime, '01/' + CONVERT(VarChar, MONTH(he.DataPrevistaEntrega)) + '/' + CONVERT(VarChar, YEAR(he.DataPrevistaEntrega)), 103) AS DataFormatacao,
				 YEAR(he.DataPrevistaEntrega) AS Ano
		    FROM {0}.{1}.Projeto AS p INNER JOIN								 
				 {0}.{1}.Entrega AS e ON (e.CodigoProjeto = p.CodigoProjeto) INNER JOIN                   
                 {0}.{1}.HistoricoEntrega AS he ON (he.CodigoEntrega = e.CodigoEntrega) INNER JOIN
                 {0}.{1}.usuario AS u ON (u.CodigoUsuario = p.CodigoGerenteProjeto) INNER JOIN
                 {0}.{1}.f_GetProjetosUsuario({4},{5}, {6}) as PU ON (PU.CodigoProjeto = p.CodigoProjeto) 
                 INNER JOIN {0}.{1}.[vi_Tarefa]		AS [t]		ON (t.[CodigoProjeto]	= p.[CodigoProjeto] and CONVERT(varchar, t.CodigoTarefa) = CONVERT(varchar, e.TaskUID) AND t.IndicaMarco = 1)
                 LEFT JOIN {0}.{1}.[linkProjeto] lp on (lp.CodigoProjetoFilho = p.CodigoProjeto )
           WHERE he.DataPrevistaEntrega >= CONVERT(DateTime, '{2}', 103) 
             AND he.DataPrevistaEntrega <= CONVERT(DateTime, '{3}', 103)
           ORDER BY p.NomeProjeto, DataPrevistaEntrega
", bancodb, Ownerdb, dataPrevistaInicial, dataPrevistaFinal, codigoUsuario, codigoEntidade, codigoCarteira);
        return getDataSet(comandoSQL);
    }

    public DataSet getRelatorioAnaliseProjetos(int codigoEntidade, int codigoCarteira, int codigoUsuario)
    {
        string comandoSQL = string.Format(@" SELECT * FROM {0}.{1}.f_GetDadosOLAP_AnaliseProjetos({3}, {2}, {4}) "
                , bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);

        return getDataSet(comandoSQL);

    }

    public DataSet getRelatorioAnaliseFinanceira(int codigoEntidade, int codigoCarteira, int codigoUsuario)
    {

        string comandoSQL = string.Format(@"
DECLARE @CodigoCarteira INT
DECLARE @CodigoEntidade INT
DECLARE @CodigoUsuario INT

    SET @CodigoEntidade = {2}
    SET @CodigoUsuario = {3}
    SET @CodigoCarteira = {4}
    
 SELECT fin._Ano,
        fin._CodigoProjeto,
        fin.CodigoReservadoCR,
        fin.CodigoReservadoGrupoConta,
        fin.DescricaoGrupoConta,
        fin.CodigoReservadoConta,
        fin.DescricaoConta,
        fin.Custo,
        fin.CustoPrevisto,
        fin.CustoReal,
        fin.Mes,
        fin.NomeEntidade,
        fin.NomeProjeto,
        fin.NomePrograma,
        fin.NomeUnidadeNegocio,
        fin.Receita,
        fin.ReceitaPrevista,
        fin.ReceitaReal,
        fin.StatusProjeto,
        fin.ValorAgregadoCusto,
        fin.ValorPlanejadoCusto,
        fin.CustoPrevistoData,
        fin.ReceitaPrevistaData
   FROM {0}.{1}.f_GetFinanceiroProjetos(@CodigoEntidade,@CodigoUsuario, @CodigoCarteira) fin",
                                           bancodb, Ownerdb, codigoEntidade, codigoUsuario, codigoCarteira);


        return getDataSet(comandoSQL);

    }

    public DataSet getDadosFinanceiro(int idProjeto, string tipo, string where)
    {
        string comandoSQL = string.Format(
       @"BEGIN
             DECLARE @CodigoProjetoParam INT, -- Parâmetro             
                     @DespesaReceitaParam Char(1)
                     
             SET @CodigoProjetoParam = {2}
             SET @DespesaReceitaParam = '{3}'
                                         
	         SELECT Ano,
                    CASE Mes 
                         WHEN 1 then 'Janeiro'
                         WHEN 2 then 'Fevereiro'
                         WHEN 3 then 'Março'
                         WHEN 4 then 'Abril'
                         WHEN 5 then 'Maio'
                         WHEN 6 then 'Junho'
                         WHEN 7 then 'Julho'
                         WHEN 8 then 'Agosto'
                         WHEN 9 then 'Setembro'
                         WHEN 10 then 'Outubro'
                         WHEN 11 then 'Novembro'
                         WHEN 12 then 'Dezembro'
                       END as Mes,
		         DescricaoGrupoConta AS Grupo,
                 DescricaoConta AS Conta,
                 CASE DespesaReceita WHEN 'D' THEN 'Despesas' WHEN 'R' then 'Receitas' END AS DespesaReceita,
                 SUM(ValorTendencia) AS Valor,
                 SUM(ValorOrcado) AS ValorLB,
                 SUM(ValorReal) AS ValorReal,
                 SUM(ValorHE) AS ValorHE,
                 Sum(ValorTendencia - ValorOrcado) AS VariacaoValor,
                 Sum(ValorTendencia - ValorReal) AS ValorRestante        ,
                 NomeCR,
                 SUM(ValorPrevistoData) as ValorPrevistoData
            FROM {0}.{1}.f_GetOrcamentoProjetoComCR(@CodigoProjetoParam,year(GETDATE()))
           WHERE DespesaReceita = @DespesaReceitaParam
           GROUP BY Ano,
                    Mes,
                    DescricaoGrupoConta,
                    DescricaoConta,
                    DespesaReceita,
                    NomeCR 
           HAVING SUM(ValorTendencia) <> 0 OR
			      SUM(ValorOrcado) <> 0 OR
			      SUM(ValorReal) <> 0 OR
                  SUM(ValorPrevistoData) <> 0 			      
        END          ", bancodb, Ownerdb, idProjeto, tipo, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFinanceiroProjeto(int idProjeto, string tipo, string where)
    {
        string comandoSQL = string.Format(
       @"BEGIN
             DECLARE @CodigoProjetoParam INT, -- Parâmetro                     
                     @DespesaReceitaParam Char(1)
                     
             SET @CodigoProjetoParam = {2}
             SET @DespesaReceitaParam = '{3}'
            
             SELECT DescricaoGrupoConta AS Grupo,
                    SUM(ISNULL(ValorTendencia, 0)) AS Valor,
                    SUM(ISNULL(ValorOrcado, 0)) AS ValorLB,
                    SUM(ISNULL(ValorReal, 0)) AS ValorReal,
                    SUM(ISNULL(ValorHE, 0)) AS ValorHE,
                    Sum(ISNULL(ValorTendencia, 0) - ISNULL(ValorOrcado, 0)) AS VariacaoValor,
                    Sum(ISNULL(ValorTendencia, 0) - ISNULL(ValorReal, 0)) AS ValorRestante        
              FROM {0}.{1}.f_GetOrcamentoProjeto(@CodigoProjetoParam,year(GETDATE()))
             WHERE DespesaReceita = @DespesaReceitaParam           
             GROUP BY DescricaoGrupoConta
        END ", bancodb, Ownerdb, idProjeto, tipo, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getFinanceiroPrevistoRealProjeto(int idProjeto, int diretoria, int area, int centroCusto, string mesAnoInicio, string mesAnoTermino, int codigoPrevisao, string indicaEntradaSaida, string where)
    {
        string comandoSQL = string.Format(
       @"SELECT * FROM {0}.{1}.f_GetFinanceiroPrevistoRealizado({2}, {3}, {4}, {5}, '{6}', '{7}', {8}, '{9}')",
                                                                                                               bancodb
                                                                                                               , Ownerdb
                                                                                                               , idProjeto
                                                                                                               , diretoria
                                                                                                               , area
                                                                                                               , centroCusto
                                                                                                               , mesAnoInicio
                                                                                                               , mesAnoTermino
                                                                                                               , codigoPrevisao
                                                                                                               , indicaEntradaSaida
                                                                                                               , where);

        return getDataSet(comandoSQL);
    }

    public string getFinanceiroGraficoPrevistoReal(DataTable dt, string colunaPrevisto, string colunaReal, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();
        StringBuilder xmlAux = new StringBuilder();
        int i = 0;

        try
        {

            int codigoUsuarioResponsavel = int.Parse(getInfoSistema("IDUsuarioLogado").ToString());
            int codigoEntidadeUsuarioResponsavel = int.Parse(getInfoSistema("CodigoEntidade").ToString());

            xmlAux.Append("<categories>");

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append("<category label=\"" + dt.Rows[i]["Descricao"] + "\"/>");

            }

            xmlAux.Append("</categories>");

            //gera as colunas de projetos satisfatórios para cada entidade
            xmlAux.Append("<dataset seriesName=\"Previsto\" color=\"" + corPrevisto + "\">");

            string link = "";

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Previsto"" value=""{0}"" {1}/>", dt.Rows[i][colunaPrevisto].ToString()
                                                                                          , link));
            }

            xmlAux.Append("</dataset>");

            //gera as colunas de projetos em atenção para cada entidade
            xmlAux.Append("<dataset seriesName=\"Realizado\" color=\"" + corReal + "\">");

            //percorre todo o DataTable
            for (i = 0; i < dt.Rows.Count; i++)
            {
                xmlAux.Append(string.Format(@"<set label=""Realizado"" value=""{0}"" {1}/>", dt.Rows[i][colunaReal].ToString()
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
        xml.Append("<chart use3DLighting=\"0\" showShadow=\"0\" labelDisplay=\"WRAP\" caption=\"" + titulo + "\" palette=\"1\" inDecimalSeparator=\",\" scrollHeight=\"15\" yAxisNamePadding=\"4\" chartLeftMargin=\"4\"" +
            " legendPadding=\"2\" showBorder=\"0\" baseFontSize=\"" + fonte + "\" " + //numVisiblePlot=\"10\"" +
            " chartRightMargin=\"20\" showLegend=\"1\" BgColor=\"F7F7F7\" formatNumberScale=\"0\" thousandSeparator=\".\" inThousandSeparator=\".\"" +
            " canvasBgColor=\"F7F7F7\" chartTopMargin=\"10\" numVisiblePlot=\"20\" canvasBorderThickness=\"1\" canvasBorderColor=\"A1A1A1\" " +
            " decimalSeparator=\",\" " + usarGradiente + usarBordasArredondadas + exportar + " shownames=\"1\" adjustDiv=\"1\" showvalues=\"0\" decimals=\"2\" chartBottomMargin=\"10\">");

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

    public DataSet getDiretoriasPlanoContas(int codigoEntidade, string where)
    {
        string comandoSQL = string.Format(
       @"SELECT pcDiretoria.CodigoConta, pcDiretoria.DescricaoConta, pcDiretoria.CodigoReservadoGrupoConta 
          FROM {0}.{1}.PlanoContasFluxoCaixa AS pcDiretoria
         WHERE pcDiretoria.CodigoContaSuperior IS NULL
           AND pcDiretoria.CodigoEntidade = {2}
           {3}
           ORDER BY pcDiretoria.DescricaoConta",
                                                        bancodb
                                                        , Ownerdb
                                                        , codigoEntidade
                                                        , where);

        return getDataSet(comandoSQL);
    }

    public DataSet getAreasPlanoContas(int codigoEntidade, int codigoDiretoria, string where)
    {
        string comandoSQL = string.Format(
        @"SELECT pcArea.CodigoConta, pcArea.DescricaoConta, pcArea.CodigoReservadoGrupoConta 
            FROM {0}.{1}.PlanoContasFluxoCaixa AS pcArea INNER JOIN
			     {0}.{1}.PlanoContasFluxoCaixa AS pcDiretoria ON (pcDiretoria.CodigoConta = pcArea.CodigoContaSuperior
																  AND pcDiretoria.CodigoContaSuperior IS NULL
                                                                  AND pcDiretoria.CodigoConta = {3})
           WHERE pcArea.CodigoEntidade = {2}
            {4}
           ORDER BY pcArea.DescricaoConta",
                                                        bancodb
                                                        , Ownerdb
                                                        , codigoEntidade
                                                        , codigoDiretoria
                                                        , where);

        return getDataSet(comandoSQL);
    }

    public DataSet getCentrosCustoPlanoContas(int codigoEntidade, int codigoArea, string where)
    {
        string comandoSQL = string.Format(
        @"SELECT pcConta.CodigoConta, pcConta.DescricaoConta, pcConta.CodigoReservadoGrupoConta 
            FROM {0}.{1}.PlanoContasFluxoCaixa AS pcConta INNER JOIN
			     {0}.{1}.PlanoContasFluxoCaixa AS pcArea ON (pcArea.CodigoConta = pcConta.CodigoContaSuperior 
                                                         AND pcArea.CodigoContaSuperior IS NOT NULL
                                                         AND pcArea.CodigoConta = {3})
           WHERE pcConta.CodigoEntidade = {2}
            {4}
           ORDER BY pcConta.CodigoReservadoGrupoConta",
                                                        bancodb
                                                        , Ownerdb
                                                        , codigoEntidade
                                                        , codigoArea
                                                        , where);

        return getDataSet(comandoSQL);
    }

    public string getGraficoFinanceiroProjetoPizza(DataTable dt, string titulo, int fonte)
    {
        //Cria as variáveis para a formação do XML
        StringBuilder xml = new StringBuilder();

        string exportar = "";

        if (fonte > 12)
        {
            exportar = @" exportEnabled=""0"" showAboutMenuItem=""0""  exportFileName=""Gráfico"" exportDialogMessage=""Exportando..."" exportAtClient=""1"" exportFormats=""PDF=Exportar para PDF|PNG=Exportar para PNG|JPG=Exportar para JPG"" exportHandler=""fcExporter1""";
        }
        else
        {
            exportar = @" exportEnabled=""0""  showAboutMenuItem=""0"" ";
        }
        try
        {

            //configuração geral do gráfico
            xml.Append(string.Format(@"<chart caption=""{0}"" adjustDiv=""1""  showLegend=""0"" formatNumberScale=""0""
                  decimalSeparator="","" thousandSeparator=""."" inDecimalSeparator="","" inThousandSeparator="".""
                  BgColor=""F7F7F7"" showZeroPies=""0"" canvasBgColor=""F7F7F7"" decimals=""2"" canvasBorderThickness=""1"" canvasBorderColor=""A1A1A1"" 
                  chartTopMargin=""5"" chartRightMargin=""8"" chartBottomMargin=""2"" chartLeftMargin=""4"" formatNumber=""1""
                  showShadow=""0"" {2} showBorder=""0"" baseFontSize=""{1}"" labelDistance=""1"" enablesmartlabels=""0"" numberPrefix=""R$"" >", titulo, fonte, usarGradiente + usarBordasArredondadas + exportar));

            int i = 0;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                xml.Append(string.Format(@"<set label=""{0}"" value=""{1}"" />", dt.Rows[i]["Grupo"].ToString(), dt.Rows[i]["ValorLB"].ToString()));

            }
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

    public DataSet getUsuariosSelecionados(int idProjeto)
    {

        string comandoSQL = string.Format(
        @"SELECT Codigo,Nome 
            FROM {0}.{1}.[f_GetInteressadosProjetoPorPermissao] ({2},'{3}')
           WHERE Codigo IN(SELECT CodigoDestinatario 
                             FROM {0}.{1}.MensagemDestinatario)
", bancodb, Ownerdb, idProjeto, "RESPMENS");
        return getDataSet(comandoSQL);
    }

    /// <summary>
    /// 
    /// 31/01/2011 - by Alejandro : adiciono um novo parâmetro, cadena de string where, para fazer um filtro mais personalizado
    /// do select.
    /// </summary>
    /// <param name="idProjeto"></param>
    /// <param name="idUsuarioSolicitacao"></param>
    /// <returns></returns>
    public DataSet getUsuariosDisponiveis(int idProjeto, string tipoAssociacao, string where)
    {

        string comandoSQL = string.Format(@"
                        DECLARE @CodigoEntidade Int
                        SELECT @CodigoEntidade = [CodigoEntidade] FROM {0}.{1}.[Projeto] WHERE [CodigoProjeto] = {2}
                        SELECT
                                u.[CodigoUsuario]   AS [Codigo]
                            , u.[NomeUsuario]		AS [Nome]
                            , 'S'					AS [Disponivel]
                        FROM 
						    {0}.{1}.[Usuario]			AS [u]
						WHERE 
						        [u].[CodigoUsuario] in (SELECT f.[CodigoUsuario] FROM {0}.{1}.f_GetPossiveisDestinatariosMensagem('{3}', {2}, @CodigoEntidade) as f )
                          {4}
                        ORDER BY U.NomeUsuario
                        ", bancodb, Ownerdb, idProjeto, tipoAssociacao, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getUsuariosDisponiveisAlertas(int idProjeto, string tipoAssociacao, string where)
    {

        string comandoSQL = string.Format(@"
                        DECLARE @CodigoEntidade Int
                        SELECT @CodigoEntidade = [CodigoEntidade] FROM {0}.{1}.[Projeto] WHERE [CodigoProjeto] = {2}
                        SELECT
                                u.[CodigoUsuario]   AS [Codigo]
                            , u.[NomeUsuario]		AS [Nome]
                            , 'S'					AS [Disponivel]
                        FROM 
						    {0}.{1}.[Usuario]			AS [u]
						WHERE 
						        [u].[CodigoUsuario] in (SELECT f.[CodigoUsuario] FROM {0}.{1}.f_GetPossiveisDestinatariosMensagem('{3}', {2}, @CodigoEntidade) as f )
                          {4}
                        ORDER BY U.NomeUsuario
                        ", bancodb, Ownerdb, idProjeto, tipoAssociacao, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getListaUsuariosDestinoMensagem(int codigoMensagem, string whereAux, string orderBy)
    {

        string comandoSQL = string.Format(@"
                        SELECT      msgdes.*
                                ,	usu.NomeUsuario

                        FROM        {0}.{1}.MensagemDestinatario    AS msgdes
                        INNER JOIN  {0}.{1}.Usuario                 AS usu		ON ( msgdes.CodigoDestinatario = usu.CodigoUsuario )

                        WHERE CodigoMensagem = {2}


                        ", bancodb, Ownerdb, codigoMensagem, whereAux, orderBy);
        return getDataSet(comandoSQL);
    }

    public bool incluiMensagemProjeto(int codigoEntidade, int idProjeto, int codigoUsuarioInclusao
                                      , string mensagem, DateTime dataLimiteResposta, bool respostaNecessaria
                                      , string[] listaUsuariosSelecionados, string iniciaisTipoAssociacao, ref string mensagemErro)
    {
        bool retorno = false;
        string dateAsString;

        // se reposta necessária e a data limite contiver um valor que não seja MinValue, registra a data.
        // Caso contrário, deixa sem data.
        if ((respostaNecessaria) && (dataLimiteResposta != DateTime.MinValue))
            dateAsString = "CONVERT(DATETIME, '" + dataLimiteResposta.Day + "/" + dataLimiteResposta.Month + "/" + dataLimiteResposta.Year + "', 103)";
        else
            dateAsString = "NULL";

        string insereDestinatarios = "";
        for (int j = 0; j < listaUsuariosSelecionados.Length; j++)
        {
            if (listaUsuariosSelecionados[j] != "")
                insereDestinatarios += string.Format(@"
                    INSERT INTO {0}.{1}.[MensagemDestinatario]
                        ([CodigoMensagem]
                        ,[CodigoDestinatario])
                    VALUES
                        (@CodigoMensagem
                        ,{2});", bancodb, Ownerdb, listaUsuariosSelecionados[j]);
        }

        string comandoSQL = string.Format(@"
        BEGIN
            DECLARE 
                    @CodigoMensagem             Int
                ,   @CodigoTipoAssociacao       SmallInt
            SELECT @CodigoTipoAssociacao = [CodigoTipoAssociacao] FROM {0}.{1}.[TipoAssociacao] WHERE [IniciaisTipoAssociacao] = '{9}'

            INSERT INTO {0}.{1}.[Mensagem]
                (     [Mensagem]
                    , [DataInclusao]
                    , [IndicaRespostaNecessaria]
                    , [CodigoUsuarioInclusao]
                    , [CodigoEntidade]
                    , [CodigoObjetoAssociado]
                    , [CodigoTipoAssociacao]
                    , [DataLimiteResposta]
                )
            VALUES
                ( '{2}', GETDATE(), '{3}' , {4}, {5} , {6} , @CodigoTipoAssociacao, {7} )
         
             SELECT @CodigoMensagem = SCOPE_IDENTITY()
 
           {8}
        END
        ", bancodb, Ownerdb, mensagem, (respostaNecessaria) ? "S" : "N", codigoUsuarioInclusao, codigoEntidade
         , idProjeto, dateAsString, insereDestinatarios, iniciaisTipoAssociacao);
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

    //By Alejandro - 28/01/2011
    /// <summary>
    /// Obtem as inicias do TipoAssociação de um mensagem detereminado (codigoMensagem).
    /// </summary>
    /// <param name="codigoMensagem"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    public string getIniciaisTipoObjetoMensagem(int codigoMensagem, string where)
    {
        string retorno = "";
        comandoSQL = string.Format(@"
                    DECLARE @IniciaisTipoObjetoMensagem	 Char(2)

                    SELECT @IniciaisTipoObjetoMensagem = ta.IniciaisTipoAssociacao 

                    FROM        {0}.{1}.Mensagem        AS m 
                    INNER JOIN  {0}.{1}.TipoAssociacao  AS ta ON ta.CodigoTipoAssociacao	= m.CodigoTipoAssociacao

                    WHERE m.CodigoMensagem	= {2}
                    {3}

                    SELECT @IniciaisTipoObjetoMensagem	AS iniciaisTipoAssociacao
                    ", bancodb, Ownerdb, codigoMensagem, where);

        DataSet ds = getDataSet(comandoSQL);
        if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
        {
            retorno = ds.Tables[0].Rows[0]["iniciaisTipoAssociacao"].ToString();
        }

        return retorno;
    }

    /// <summary>
    /// Devolve um DataSet com os dados do projeto
    /// </summary>
    /// <param name="IniciaisTipoObjetoParam">Iniiciais do tipo de objeto</param>
    /// <param name="CodigoUsuarioParam">Código do Usuário</param>
    /// <param name="CodigoObjetoParam">Código do Objeto</param>
    /// <param name="StatusParam">Status da mensagem - filtro</param>
    /// <param name="MensagensEnvieiParam">Mensagens que enviei - filtro</param>
    /// <param name="MensagensRespondoParam">Mensagens que respondo - filtro</param>
    /// <param name="erro">se a função retornar um erro, este será armazenado nesse parâmetro</param>
    /// <returns>DataSet</returns>
    public DataSet getMensagemObjeto(string IniciaisTipoObjetoParam, int CodigoUsuarioParam, int CodigoObjetoParam
                                    , string StatusParam, string MensagensEnvieiParam, string MensagensRespondoParam
                                    , string where, ref string erro)
    {
        /*{2}--CodigoProjetoParam
		,{3}--CodigoUsuarioParam
		,'{4}'--StatusParam
		,'{5}'--MensagensEnvieiParam
		,'{6}'--MensagensRespondoParam*/
        string comandoSQL = string.Format(@"
                SELECT      m.*
                        ,   p.NomeProjeto
                        ,   u.NomeUsuario AS UsuarioInclusao
                        ,   getmsg.CodigoUsuarioInclusao
                        ,   getmsg.ExcluiMensagem
                        ,   getmsg.EditaMensagem
                        ,   getmsg.EditaResposta
                        ,   (SELECT     TOP 1 u.NomeUsuario 
                                FROM    {0}.{1}.Usuario u 
                                WHERE   u.CodigoUsuario = m.CodigoUsuarioResposta) AS NomeUsuarioResposta

                FROM        {0}.{1}.f_GetMensagens('{2}',{3},{4},'{5}','{6}','{7}') AS getmsg
                INNER JOIN  {0}.{1}.Mensagem    AS m ON getmsg.CodigoMensagem           = m.CodigoMensagem
                INNER JOIN  {0}.{1}.Usuario     AS u ON getmsg.CodigoUsuarioInclusao    = u.CodigoUsuario
                LEFT JOIN   {0}.{1}.Projeto     AS p ON p.CodigoProjeto                 = m.CodigoObjetoAssociado
                
                WHERE 1=1
                {8}

                ORDER BY m.DataInclusao DESC
        ", bancodb, Ownerdb, IniciaisTipoObjetoParam, CodigoObjetoParam
         , CodigoUsuarioParam, StatusParam, MensagensEnvieiParam
         , MensagensRespondoParam, where);
        return getDataSet(comandoSQL);
    }

    public DataSet getMensagemProjetoSimples(string where)
    {
        string comandoSQL = string.Format(@"
        SELECT M.CodigoMensagem, 
               M.Mensagem, 
               M.DataInclusao, 
               M.Resposta, 
               M.DataResposta, 
               M.IndicaRespostaNecessaria,
               M.CodigoUsuarioInclusao,
               M.CodigoUsuarioResposta,
               M.CodigoEntidade,
               M.CodigoObjetoAssociado,
               M.CodigoTipoAssociacao,
               M.DataLimiteResposta
         FROM {0}.{1}.Mensagem M 
   INNER JOIN  {0}.{1}.MensagemDestinatario MD 
    LEFT JOIN {0}.{1}.[f_GetMensagensProjeto](CodigoObjetoAssociado,CodigoUsuarioInclusao,'Todas','N','N') fgm
        WHERE (1=1) {2}", bancodb, Ownerdb, where);
        return getDataSet(comandoSQL);
    }

    public bool editaMensagemProjeto(int codigoMensagem, int codigoEntidade, int codigoProjeto, int idUsuarioLogado, string txtMensagem, string txtResposta, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
            UPDATE {0}.{1}.[Mensagem]
               SET [Mensagem] = '{2}'
                  ,[Resposta] = '{3}'
            WHERE [CodigoMensagem] = {4}"
            , bancodb, Ownerdb, txtMensagem, txtResposta, codigoMensagem);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool excluiMensagemProjeto(int codigoMensagem, ref string msgErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
            DELETE FROM {0}.{1}.MensagemDestinatario    WHERE CodigoMensagem = {2} 
            DELETE FROM {0}.{1}.Mensagem                WHERE CodigoMensagem = {2}
            ", bancodb, Ownerdb, codigoMensagem);
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

    public bool respondeMensagemProjeto(int codMensagem, string resposta, int idUsuarioLogado, ref string msgErro)
    {
        bool retorno = false;
        try
        {
            string comandoSQL = string.Format(@"
                    UPDATE {0}.{1}.[Mensagem]
                       SET [Resposta] = '{2}'
                          ,[DataResposta] = getdate()
                          ,[CodigoUsuarioResposta] = {4}
                    WHERE CodigoMensagem = {3}", bancodb, Ownerdb, resposta, codMensagem, idUsuarioLogado);
            int regAfetados = 0;
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msgErro = ex.Message;
        }
        return retorno;
    }

    public bool atualizaDataLeituraMensagemProjeto(int codMensagem, int codigoDestinatario, ref string msg)
    {
        string comandoSQL = string.Format(@"
                                UPDATE  {0}.{1}.[MensagemDestinatario]
                                SET     [DataLeitura] = GETDATE()
                                WHERE   [CodigoMensagem]    = {2} 
                                  AND   CodigoDestinatario  = {3}

                --UPDATE  {0}.{1}.[CaixaMensagem]
                --SET     [DataPrimeiroAcessoMensagem] = getdate()
                --WHERE   CodigoCaixaMensagem = {2}
        ", bancodb, Ownerdb, codMensagem, codigoDestinatario);

        bool retorno = false;
        int regAfetados = 0;

        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch (Exception ex)
        {
            retorno = false;
            msg = ex.Message;
        }

        return retorno;
    }

    public bool atualizaCronogramaCheckin(int codigoProjeto, string CodigoCronogramaProjeto)
    {
        string where = "AND CodigoProjeto = " + codigoProjeto;

        // se informou o codigo do cronograma, este tem prioridade em relação ao código do projeto
        if (CodigoCronogramaProjeto != "")
            where = string.Format("AND CodigoCronogramaProjeto = '{0}'", CodigoCronogramaProjeto);

        string comandoSQL = string.Format(@"
        UPDATE CronogramaProjeto
           SET DataCheckoutCronograma = null
              ,CodigoUsuarioCheckoutCronograma = null
         WHERE DataExclusao is null
           {2} ", bancodb, Ownerdb, where);
        bool retorno = false;
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            retorno = false;
        }
        return retorno;

    }

    public bool atualizaAnexoCheckin(int codigoAnexo)
    {
        string comandoSQL = string.Format(@"
        BEGIN
            UPDATE {0}.{1}.Anexo
               SET dataCheckOut = NULL 
                 , codigoUsuarioCheckOut = NULL
            WHERE CodigoAnexo = {2}
              AND dataCheckIn IS NULL

            UPDATE {0}.{1}.AnexoVersao
               SET dataCheckOut = NULL 
                 , codigoUsuarioCheckOut = NULL
            WHERE CodigoSequencialAnexo = (SELECT MAX(av.CodigoSequencialAnexo) FROM {0}.{1}.AnexoVersao av WHERE av.CodigoAnexo = {2} AND av.dataCheckIn IS NULL)
              AND dataCheckIn IS NULL 

        END", bancodb, Ownerdb, codigoAnexo);
        bool retorno = false;
        int regAfetados = 0;
        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            retorno = false;
        }
        return retorno;

    }

    public DataSet getCronogramasComCheckoutComPermissao(int codigoEntidade, int codigoUsuario, string where)
    {
        string whereEntidade = " AND CP.CodigoEntidade = " + codigoEntidade;
        if (-1 == codigoEntidade)
        {
            whereEntidade = "";
        }

        comandoSQL = string.Format(@"
        SELECT 
              cp.[NomeProjeto] 
              ,u.NomeUsuario as NomeUsuarioCheckoutCronograma   
              ,cp.[DataCheckoutCronograma]                      
              ,isnull(cp.[CodigoProjeto], cpOficial.CodigoProjeto) CodigoProjeto                               
              ,cp.[CodigoCronogramaProjeto]  
              ,case when cp.CodigoProjeto is null then 'Replanejamento' else 'Oficial' end Tipo
              ,case when {0}.{1}.f_VerificaAcessoConcedido({4}, cp.CodigoEntidade, isnull(cp.[CodigoProjeto], cpOficial.CodigoProjeto),null, 'PR', 0, null, 'PR_DesBlq') = 1 
                    then 'S'
                    else 'N'
                    end AS Permissao                
           FROM CronogramaProjeto cp LEFT JOIN 
                Usuario u on u.CodigoUsuario = cp.CodigoUsuarioCheckoutCronograma  LEFT JOIN
				CronogramaProjeto cpOficial on cpOficial.CodigoCronogramaReplanejamento = cp.CodigoCronogramaProjeto AND
                                               cpOficial.DataExclusao is null
         WHERE cp.DataCheckoutCronograma IS NOT null  
           AND cp.CodigoUsuarioCheckoutCronograma IS NOT NULL 
           AND cp.DataExclusao is null 
           {2} {3}", bancodb, Ownerdb, whereEntidade, where, codigoUsuario);
        return getDataSet(comandoSQL);
    }

    public DataSet getCronogramasComCheckout(int codigoEntidade, string where, int codigoProjeto = -1)
    {
        string whereEntidade = " AND cp.CodigoEntidade = " + codigoEntidade;
        string colunaCodigoProjeto = (codigoProjeto == -1) ? "cp.CodigoProjeto" : codigoProjeto.ToString();
        if (-1 == codigoEntidade)
        {
            whereEntidade = "";
        }

        comandoSQL = string.Format(@"
		SELECT
			cp.[NomeProjeto]
			,u.CodigoUsuario as CodigoUsuarioCheckoutCronograma
			,u.NomeUsuario as NomeUsuarioCheckoutCronograma
			,cp.[DataCheckoutCronograma]
			,{4}
			,cp.[CodigoCronogramaProjeto]
	    FROM CronogramaProjeto cp 
        LEFT JOIN Usuario u on u.CodigoUsuario = cp.CodigoUsuarioCheckoutCronograma
        WHERE cp.DataCheckoutCronograma IS NOT null
          AND cp.CodigoUsuarioCheckoutCronograma IS NOT NULL
          AND cp.DataExclusao is null
           {2}
          AND cp.CodigoCronogramaOrigem IN (SELECT CodigoCronogramaOrigem 
                                              FROM CronogramaProjeto AS cpo 
                                             WHERE cpo.CodigoProjeto = {5})", bancodb, Ownerdb, whereEntidade, where, colunaCodigoProjeto, codigoProjeto);
        return getDataSet(comandoSQL);
    }

    public DataSet getAnexosComCheckout(int codigoEntidade, string where)
    {
        string whereEntidade = " AND a.CodigoEntidade = " + codigoEntidade;

        if (-1 == codigoEntidade)
        {
            whereEntidade = "";
        }

        comandoSQL = string.Format(@"
        SELECT a.CodigoAnexo, a.Nome, a.codigoUsuarioCheckOut, a.dataCheckOut, U.NomeUsuario as NomeUsuarioCheckout
          FROM {0}.{1}.Anexo a INNER JOIN
               {0}.{1}.Usuario U on U.CodigoUsuario = a.codigoUsuarioCheckOut
         WHERE dataCheckOut IS NOT NULL
           AND dataCheckIn IS NULL
           {2}
           {3}
", bancodb, Ownerdb, whereEntidade, where);
        return getDataSet(comandoSQL);
    }

    public void getPodeReativarDemandaSimples(int CodigoStatusProjeto, int CodigoResponsavelDemanda, int idEntidadLogada
                                              , ref string Resposta)
    {
        DataSet ds;
        try
        {
            string comandoSQL = string.Format(@"
                    DECLARE @CodigoStatusProjeto INT
                        ,   @CodigoStatusDMDCancelada INT
                        ,   @CodigoResponsavelProjeto INT	
                        ,   @OK Char(1)

                    SET @CodigoStatusProjeto        = {2}
                    SET @CodigoResponsavelProjeto   = {3}
                    SET @OK = 'S'

                    IF( EXISTS( SELECT 1 FROM {0}.{1}.[Status] AS [st] 
                                WHERE st.[CodigoStatus] = @CodigoStatusProjeto 
                                AND (       st.[IndicaAcompanhamentoExecucao]       = 'S' 
                                        OR  st.[IndicaAcompanhamentoPortfolio]      = 'S' 
                                        OR  st.[IndicaAcompanhamentoPosExecucao]    = 'S'
                                    )
                               ) 
                      )
                    BEGIN
                        IF( NOT EXISTS(SELECT 1 FROM {0}.{1}.[Usuario] as [us] 
                                        INNER JOIN {0}.{1}.[UsuarioUnidadeNegocio] AS uun ON us.[CodigoUsuario] = uun.[CodigoUsuario]
                                        WHERE us.[CodigoUsuario]                    = @CodigoResponsavelProjeto 
                                        AND uun.CodigoUnidadeNegocio                = {4}
                                        AND uun.[IndicaUsuarioAtivoUnidadeNegocio]	= 'S'
                                        AND us.[DataExclusao] IS NULL
                                      ) 
                           )
                        BEGIN
                            SET @OK = 'N'
                        END
                    END

                    SELECT @OK AS Resultado
            ", bancodb, Ownerdb, CodigoStatusProjeto
             , CodigoResponsavelDemanda, idEntidadLogada);

            ds = getDataSet(comandoSQL);
            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                Resposta = ds.Tables[0].Rows[0]["Resultado"].ToString();
        }
        catch (Exception ex)
        {
            Resposta = ex.Message;
        }
    }


    public bool incluiDemandaSimples(string nomeDemanda, string descricaoDemanda, string inicio, string termino, char prioridade,
                                     int codigoDemandante, int codigoUnidade, int codigoResponsavel, int codigoUsuarioLogado,
                                     int codigoEntidade, string codigoTipoProjeto, ref string mensagemErro, ref int novoCodigoDemanda)
    {
        bool retorno = false;

        inicio = inicio == "" ? "NULL" : "CONVERT(DateTime, '" + inicio + "', 103)";
        termino = termino == "" ? "NULL" : "CONVERT(DateTime, '" + termino + "', 103)";

        string demandante = codigoDemandante == -1 ? "NULL" : codigoDemandante.ToString();
        string responsavel = codigoResponsavel == -1 ? "NULL" : codigoResponsavel.ToString();
        string unidade = codigoUnidade == -1 ? "NULL" : codigoUnidade.ToString();
        int valorPrioridade;

        nomeDemanda = nomeDemanda.Replace("'", "''");
        descricaoDemanda = descricaoDemanda.Replace("'", "''");

        switch (prioridade)
        {
            case 'A':
                valorPrioridade = 100;
                break;
            case 'M':
                valorPrioridade = 300;
                break;
            default:
                valorPrioridade = 500;
                break;
        }

        string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoStatusDemanda int

                SELECT @CodigoStatusDemanda = CodigoStatus
                  FROM {0}.{1}.Status
                 WHERE TipoStatus = 'DMD'
                   AND IndicaSelecaoPortfolio = 'S'
                   AND IndicaAcompanhamentoPortfolio = 'S'
                   AND IndicaAcompanhamentoExecucao = 'S'
                   AND IndicaAcompanhamentoPosExecucao = 'N'

                INSERT INTO {0}.{1}.Projeto(NomeProjeto, DescricaoProposta, InicioProposta, TerminoProposta, 
                                            Prioridade, CodigoCliente, CodigoUnidadeNegocio, CodigoGerenteProjeto, 
                                            DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade,
                                            CodigoTipoProjeto
                                            , IndicaRecursosAtualizamTarefas
                                            , IndicaTipoAtualizacaoTarefas
                                            , IndicaAprovadorTarefas )
                                     VALUES('{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, GetDate(), {10},
                                            @CodigoStatusDemanda, 'N', {11}, {12}, 'N', 'P', 'GR')
                SELECT scope_identity()
            END
        ", bancodb, Ownerdb, nomeDemanda, descricaoDemanda, inicio, termino, valorPrioridade, demandante, unidade, responsavel, codigoUsuarioLogado, codigoEntidade, codigoTipoProjeto);

        try
        {
            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoDemanda = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            return true;
        }
        catch
        {
            retorno = false;
            mensagemErro = "Verifique se já não existe Demanda, Projeto ou Processo com esse nome!";
        }

        return retorno;

    }

    public bool atualizaDemandaSimples(int codigoDemanda, string nomeDemanda, string descricaoDemanda, string inicio, string termino, char prioridade,
                                     int codigoDemandante, int codigoUnidade, int codigoStatus, int codigoResponsavel, int codigoUsuarioLogado, string comentario, ref string mensagemErro)
    {
        bool retorno = false;

        inicio = inicio == "" ? "NULL" : "CONVERT(DateTime, '" + inicio + "', 103)";
        termino = termino == "" ? "NULL" : "CONVERT(DateTime, '" + termino + "', 103)";

        string demandante = codigoDemandante == -1 ? "NULL" : codigoDemandante.ToString();
        string responsavel = codigoResponsavel == -1 ? "NULL" : codigoResponsavel.ToString();
        string unidade = codigoUnidade == -1 ? "NULL" : codigoUnidade.ToString();
        int valorPrioridade;

        nomeDemanda = nomeDemanda.Replace("'", "''");
        descricaoDemanda = descricaoDemanda.Replace("'", "''");
        comentario = comentario.Replace("'", "''");

        switch (prioridade)
        {
            case 'A':
                valorPrioridade = 100;
                break;
            case 'M':
                valorPrioridade = 300;
                break;
            default:
                valorPrioridade = 500;
                break;
        }

        string comandoSQL = string.Format(@"
            UPDATE {0}.{1}.Projeto SET NomeProjeto = '{2}'
                                     , DescricaoProposta = '{3}'
                                     , InicioProposta = {4}
                                     , TerminoProposta = {5}
                                     , Prioridade = {6}
                                     , CodigoCliente = {7}
                                     , CodigoUnidadeNegocio = {8}
                                     , CodigoGerenteProjeto = {9}
                                     , CodigoUsuarioUltimaAlteracao = {11}
                                     , DataUltimaAlteracao = GetDate()
                                     , CodigoStatusProjeto = {12}
                                     , Comentario = '{13}'
                                 WHERE CodigoProjeto = {10}
        ", bancodb, Ownerdb, nomeDemanda, descricaoDemanda, inicio, termino
         , valorPrioridade, demandante, unidade, responsavel, codigoDemanda
         , codigoUsuarioLogado, codigoStatus, comentario);

        int regAfetados = 0;

        try
        {
            retorno = execSQL(comandoSQL, ref regAfetados);
        }
        catch
        {
            retorno = false;
            mensagemErro = "Verifique se já não existe Demanda, Projeto ou Processo com esse nome!";
        }

        return retorno;

    }


    public bool incluiDemandaComplexa(string nomeDemanda, string descricaoDemanda, string inicio, string termino, char prioridade,
                                     int codigoDemandante, int codigoUnidade, int codigoResponsavel, int codigoUsuarioLogado,
                                     int codigoEntidade, string codigoTipoProjeto, ref string mensagemErro, ref int novoCodigoDemanda)
    {
        bool retorno = false;

        inicio = inicio == "" ? "NULL" : "CONVERT(DateTime, '" + inicio + "', 103)";
        termino = termino == "" ? "NULL" : "CONVERT(DateTime, '" + termino + "', 103)";

        string demandante = codigoDemandante == -1 ? "NULL" : codigoDemandante.ToString();
        string responsavel = codigoResponsavel == -1 ? "NULL" : codigoResponsavel.ToString();
        string unidade = codigoUnidade == -1 ? "NULL" : codigoUnidade.ToString();
        int valorPrioridade;

        nomeDemanda = nomeDemanda.Replace("'", "''");
        descricaoDemanda = descricaoDemanda.Replace("'", "''");

        switch (prioridade)
        {
            case 'A':
                valorPrioridade = 100;
                break;
            case 'M':
                valorPrioridade = 300;
                break;
            default:
                valorPrioridade = 500;
                break;
        }

        string comandoSQL = string.Format(@"
            BEGIN
                DECLARE @CodigoStatusDemanda int
                
                -- assumindo status 'Aguardando Análise' 
                SELECT @CodigoStatusDemanda = 14

                INSERT INTO {0}.{1}.Projeto(NomeProjeto, DescricaoProposta, InicioProposta, TerminoProposta, 
                                            Prioridade, CodigoCliente, CodigoUnidadeNegocio, CodigoGerenteProjeto, 
                                            DataInclusao, CodigoUsuarioInclusao, CodigoStatusProjeto, IndicaPrograma, CodigoEntidade,
                                            CodigoTipoProjeto
                                            , IndicaRecursosAtualizamTarefas
                                            , IndicaTipoAtualizacaoTarefas
                                            , IndicaAprovadorTarefas )
                                     VALUES('{2}', '{3}', {4}, {5}, {6}, {7}, {8}, {9}, GetDate(), {10},
                                            @CodigoStatusDemanda, 'N', {11}, {12}, 'N', 'P', 'GR')
                SELECT scope_identity()
            END
        ", bancodb, Ownerdb, nomeDemanda, descricaoDemanda, inicio, termino, valorPrioridade, demandante, unidade, responsavel, codigoUsuarioLogado, codigoEntidade, codigoTipoProjeto);


        try
        {
            DataSet ds = getDataSet(comandoSQL);

            if (DataSetOk(ds) && DataTableOk(ds.Tables[0]))
                novoCodigoDemanda = int.Parse(ds.Tables[0].Rows[0][0].ToString());

            return true;
        }
        catch
        {
            retorno = false;
            mensagemErro = "Verifique se já não existe Demanda, Projeto ou Processo com esse nome!";
        }

        return retorno;

    }


    #region Ítens do backlog

    public DataSet getItensDoBackLog(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
        SELECT ib.[CodigoItem]
              ,ib.[CodigoProjeto]
              ,ib.[CodigoItemSuperior]
              ,ib.[TituloItem]
              ,ib.[DetalheItem]
              ,ib.[CodigoTipoStatusItem]
              ,ts.DescricaoTipoStatusItem
              ,ib.[CodigoTipoClassificacaoItem]
              ,tc.DescricaoTipoClassificacaoItem
              ,ib.[CodigoUsuarioInclusao]
              ,ib.[DataInclusao]
              ,ib.[CodigoUsuarioUltimaAlteracao]
              ,ib.[DataUltimaAlteracao]
              ,ib.[CodigoUsuarioExclusao]
              ,ib.[PercentualConcluido]
              ,ib.[CodigoIteracao]
              ,ib.[Importancia]
              ,ib.[Complexidade]
              ,(SELECT CASE ib.[Complexidade]
                 WHEN 0 THEN 'Baixa'
                 WHEN 1 THEN 'Média'
                 WHEN 2 THEN 'Alta'
                 WHEN 3 THEN 'Muito Alta'
               END) AS DescricaoComplexidade
              ,ib.[EsforcoPrevisto]
              ,ib.[IndicaItemNaoPlanejado]
              ,ib.[IndicaQuestao]
              ,ib.[IndicaBloqueioItem]
              ,ib.[CodigoWorkflow]
              ,ib.[CodigoInstanciaWF]
              ,ib.[CodigoCronogramaProjetoReferencia]
              ,ib.[CodigoTarefaReferencia]
              ,p.CodigoPessoa as CodigoCliente
              ,p.NomePessoa as NomeCliente
              ,ttf.CodigoTipoTarefaTimeSheet
              ,ttf.DescricaoTipoTarefaTimeSheet
              ,ib.ReceitaPrevista
              ,(SELECT p.NomeProjeto  
                  FROM {0}.{1}.Agil_Iteracao AS i INNER JOIN
                       {0}.{1}.Projeto AS p ON (i.CodigoProjetoIteracao = p.CodigoProjeto) INNER JOIN
                       {0}.{1}.Agil_ItemBacklog As ai ON  (ai.CodigoIteracao = i.CodigoIteracao)
                 WHERE ai.CodigoItem = ib.[CodigoItem]) as Sprint
               ,ts.TituloStatusItem as TituloStatusItem
         FROM {0}.{1}.[Agil_ItemBacklog] ib
         LEFT JOIN {0}.{1}.Agil_TipoStatusItemBacklog ts on (ib.CodigoTipoStatusItem = ts.CodigoTipoStatusItem)
         LEFT JOIN {0}.{1}.Agil_TipoClassificacaoItemBacklog tc on (ib.CodigoTipoClassificacaoItem = tc.CodigoTipoClassificacaoItem)
         LEFT JOIN {0}.{1}.Pessoa p on (p.CodigoPessoa = ib.CodigoPessoa)
         LEFT JOIN {0}.{1}.TipoTarefaTimeSheet AS ttf on (ttf.CodigoTipoTarefaTimeSheet = ib.CodigoTipoTarefaTimesheet) 
         WHERE ib.CodigoProjeto = {2} AND ib.[CodigoItemSuperior] IS NULL
        ORDER BY ib.[Importancia] desc
            {3}", getDbName(), getDbOwner(), codigoProjeto, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet incluiItensBackLog(int CodigoProjeto, string TituloItem, string DetalheItem, string CodigoTipoStatusItem,
        int CodigoTipoClassificacaoItem, int CodigoUsuarioInclusao, int Importancia, int Complexidade,
        decimal EsforcoPrevisto, string CodigoCronogramaProjetoReferencia, int codigoCliente, int codigoTipoTarefaTimesheet, decimal receitaPrevista)
    {
        string ins_codigoCliente = (codigoCliente == -1) ? "NULL" : codigoCliente.ToString();
        string ins_codigoTipoTarefaTimesheet = (codigoTipoTarefaTimesheet == -1) ? "NULL" : codigoTipoTarefaTimesheet.ToString();
        string ins_receitaPrevista = (receitaPrevista == 0) ? "NULL" : receitaPrevista.ToString().Replace(".", "").Replace(",", ".");
        string ins_EsforcoPrevisto = (EsforcoPrevisto == 0) ? "NULL" : EsforcoPrevisto.ToString().Replace(".", "").Replace(",", ".");

        string comandoSQL = string.Format(@"INSERT INTO {0}.{1}.[Agil_ItemBacklog]
           ([CodigoProjeto],[TituloItem] ,[DetalheItem],[CodigoTipoStatusItem],[CodigoTipoClassificacaoItem],[CodigoUsuarioInclusao],[DataInclusao],[PercentualConcluido],[Importancia],[Complexidade],[EsforcoPrevisto],[CodigoCronogramaProjetoReferencia],[CodigoPessoa],[CodigoTipoTarefaTimesheet],[ReceitaPrevista] )
     VALUES(            {2},        '{3}',        '{4}',                   {5},                          {6},                    {7},     GETDATE(),                  0.0,          {8},           {9},             {10}, '{11}'                            ,{12}          ,{13}                       , {14})"
            , bancodb, Ownerdb
            , CodigoProjeto, TituloItem, DetalheItem, CodigoTipoStatusItem, CodigoTipoClassificacaoItem, CodigoUsuarioInclusao, Importancia, Complexidade, ins_EsforcoPrevisto, CodigoCronogramaProjetoReferencia, ins_codigoCliente, ins_codigoTipoTarefaTimesheet, ins_receitaPrevista);

        DataSet ds = getDataSet(geraBlocoBeginTran() + "  " + comandoSQL + "  " + geraBlocoEndTran());
        return ds;
    }

    public DataSet getClassificacaoItensBackLog(string where)
    {

        string comandoSQL = string.Format(@"
        SELECT [CodigoTipoClassificacaoItem]
              ,[DescricaoTipoClassificacaoItem]
              ,[IniciaisTipoClassificacaoItemControladoSistema]
         FROM {0}.{1}.[Agil_TipoClassificacaoItemBacklog]
        WHERE 1 = 1 {2}", bancodb, Ownerdb, where);
        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet atualizaItensDoBackLog(int CodigoItem, int codigoProjeto, string tituloItem, string detalheItem, int CodigoTipoStatusItem, int CodigoTipoClassificacaoItem
        , int CodigoUsuarioUltimaAlteracao, int Importancia, int Complexidade, decimal EsforcoPrevisto, string CodigoCronogramaProjetoReferencia, int codigoCliente, int codigoTipoTarefaTimesheet, decimal receitaPrevista)
    {

        string ins_receitaPrevista = (receitaPrevista == 0) ? "NULL" : receitaPrevista.ToString().Replace(".", "").Replace(",", ".");
        string ins_EsforcoPrevisto = (EsforcoPrevisto == 0) ? "NULL" : EsforcoPrevisto.ToString().Replace(".", "").Replace(",", ".");


        string inicioComandoUpdate = string.Format(@"UPDATE {0}.{1}.[Agil_ItemBacklog]", getDbName(), getDbOwner());
        string sets = " SET ";
        sets += string.Format(" [CodigoProjeto] = {0} ", codigoProjeto);
        sets += string.Format(" ,[TituloItem] = '{0}' ", tituloItem);
        sets += string.Format(" ,[DetalheItem] = '{0}' ", detalheItem);
        sets += string.Format(" ,[CodigoTipoStatusItem] = {0} ", CodigoTipoStatusItem);
        sets += string.Format(" ,[CodigoTipoClassificacaoItem] = {0} ", CodigoTipoClassificacaoItem);
        sets += string.Format(" ,[CodigoUsuarioUltimaAlteracao] = {0} ", CodigoUsuarioUltimaAlteracao);
        sets += " ,[DataUltimaAlteracao] = getdate() ";
        sets += string.Format(" ,[Importancia] = {0} ", Importancia);
        sets += string.Format(" ,[Complexidade] = {0} ", Complexidade);
        sets += string.Format(" ,[EsforcoPrevisto] = {0} ", ins_EsforcoPrevisto);
        sets += string.Format(" ,[ReceitaPrevista] = {0} ", ins_receitaPrevista);

        sets += string.Format(" ,[CodigoCronogramaProjetoReferencia] = '{0}' ", CodigoCronogramaProjetoReferencia);

        sets += (codigoCliente == -1) ? " ,[CodigoPessoa] = NULL " : string.Format(" ,[CodigoPessoa] = {0} ", codigoCliente);
        sets += (codigoTipoTarefaTimesheet == -1) ? " ,[codigoTipoTarefaTimesheet] = NULL " : string.Format(" ,[codigoTipoTarefaTimesheet] = {0} ", codigoTipoTarefaTimesheet);

        string wheres = "";

        wheres += string.Format(" WHERE CodigoItem = {0} ", CodigoItem);


        DataSet ds = getDataSet(geraBlocoBeginTran() + "  " + inicioComandoUpdate + sets + wheres + geraBlocoEndTran());
        return ds;

    }

    public bool excluiItensDoBackLog(int codigoEntidade, int idUsuarioLogado, int codigoItem, ref string mensagemErro)
    {
        bool retorno = false;
        string comandoSQL = string.Format(@"
        DECLARE @RC int
        DECLARE @CodigoEntidadeContexto int
        DECLARE @CodigoUsuarioSistema int
        DECLARE @CodigoItemBacklog int
        DECLARE @SiglaContextoExclusao varchar(9)

        SET @CodigoEntidadeContexto = {0}
        SET @CodigoUsuarioSistema = {1}
        SET @CodigoItemBacklog = {2}
        SET @SiglaContextoExclusao = 'LstIblPrj'

        EXECUTE @RC = [dbo].[p_Agil_excluiItemBacklog] 
           @CodigoEntidadeContexto
          ,@CodigoUsuarioSistema
          ,@CodigoItemBacklog
          ,@SiglaContextoExclusao", codigoEntidade, idUsuarioLogado, codigoItem);
        int regAfetados = 0;
        try
        {
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

    #endregion

    #region Sprints

    public DataSet getSprints(int codigoProjeto, string where)
    {
        string comandoSQL = string.Format(@"
       

        SELECT ai.[CodigoIteracao]
              ,ai.[CodigoProjetoIteracao]
              ,p.[NomeProjeto] as Titulo
              ,ai.[InicioPrevisto] as Inicio
              ,ai.[TerminoPrevisto] as Termino
              ,{0}.{1}.f_Agil_GetStatusIteracao(ai.[CodigoIteracao], ai.[DataPublicacaoPlanejamento], ai.[TerminoReal]) as Status              
              ,u.NomeUsuario as NomeProjectOwner
              ,p.[CodigoGerenteProjeto] as CodigoProjectOwner
              , tc.NomeTarefa as NomePacoteTrabalho
              ,ai.[CodigoTarefaItemEAP] as CodigoPacoteTrabalho
              ,p.[DescricaoProposta] as Objetivos
              ,ai.FatorProdutividade 
        FROM {0}.{1}.[Projeto] p
        INNER JOIN {0}.{1}.LinkProjeto lp on (lp.CodigoProjetoFilho  = p.CodigoProjeto) 
        INNER JOIN {0}.{1}.[Agil_Iteracao] ai on (ai.CodigoProjetoIteracao = p.CodigoProjeto) 
        left JOIN {0}.{1}.[Usuario] u on (u.CodigoUsuario = p.CodigoGerenteProjeto)
        left join {0}.{1}.[TarefaCronogramaProjeto] tc on (ai.[CodigoTarefaItemEAP] = tc.CodigoTarefa and tc.CodigoCronogramaProjeto in (select CodigoCronogramaProjeto from CronogramaProjeto  where CodigoProjeto = {2}))
        where lp.CodigoProjetoPai = {2} and p.DataExclusao is null
        order by p.[NomeProjeto] asc
        {3}", getDbName(), getDbOwner(), codigoProjeto, where);

        DataSet ds = getDataSet(comandoSQL);
        return ds;
    }

    public DataSet incluiSprint(int codigoEntidadeLogada, string TituloItem, string Inicio, string Termino,
        int CodigoProjectOwner, int CodigoPacoteTrabalhoAssociado, string Objetivos, int codigoProjeto, int codigoUsuarioInclusao, int CodigoEntidade, int codigoProjetoAtual, decimal fatorProdutividade, string codigoCronogramaProjeto)
    {
        string sets = "";

        sets += string.Format(@" SET @in_Titulo = '{0}'", TituloItem);
        sets += string.Format(@" SET @in_DataInicio =  CONVERT(DateTime,'{0}',103)", Inicio);
        sets += string.Format(@" SET @in_DataTermino = CONVERT(DateTime,'{0}',103)", Termino);
        sets += string.Format(@" SET @in_codigoProjectOwner = {0}", CodigoProjectOwner);
        sets += string.Format(@" SET @in_CodigoPacoteTrabalho = {0}", CodigoPacoteTrabalhoAssociado == -1 ? "NULL" : CodigoPacoteTrabalhoAssociado.ToString());
        sets += string.Format(@" SET @in_Objetivos = '{0}'", Objetivos);
        sets += string.Format(@" SET @in_CodigoProjeto = {0}", codigoProjeto);
        sets += string.Format(@" SET @in_CodigoUsuarioInclusao = {0}", codigoUsuarioInclusao);
        sets += string.Format(@" SET @in_CodigoEntidade = {0}", CodigoEntidade);
        sets += string.Format(@" SET @in_CodigoProjetoAtual = {0}", codigoProjetoAtual);
        sets += string.Format(@" SET @in_FatorProdutividade = {0}", fatorProdutividade.ToString().Replace(",", "."));
        sets += string.Format(@" SET @in_codigoCronogramaProjeto = '{0}'", codigoCronogramaProjeto);

        comandoSQL = string.Format(@"
        DECLARE @RC int
        DECLARE @in_Titulo varchar(150)
        DECLARE @in_DataInicio datetime
        DECLARE @in_DataTermino datetime
        DECLARE @in_CodigoStatus int
        DECLARE @in_codigoProjectOwner int
        DECLARE @in_CodigoPacoteTrabalho int
        DECLARE @in_Objetivos varchar(200)
        DECLARE @in_CodigoProjeto int
        DECLARE @in_CodigoUsuarioInclusao INT
        DECLARE @in_CodigoEntidade Int
        DECLARE @in_CodigoProjetoAtual Int
        DECLARE @in_FatorProdutividade decimal(10,2)
        DECLARE @in_codigoCronogramaProjeto varchar(64)
       
        {2}

       EXECUTE @RC = {0}.{1}.[p_Agil_IncluiIteracao] 
        @in_Titulo
	    ,@in_DataInicio
	    ,@in_DataTermino
        ,@in_CodigoStatus
        ,@in_codigoProjectOwner		
	    ,@in_CodigoPacoteTrabalho
	    ,@in_Objetivos
        ,@in_CodigoUsuarioInclusao
        ,@in_CodigoEntidade
        ,@in_CodigoProjetoAtual
        ,@in_FatorProdutividade
        ,@in_codigoCronogramaProjeto

SET @CodigoRetorno = @RC
", bancodb, Ownerdb, sets);

        DataSet ds1 = getDataSet(geraBlocoBeginTran() + "  " + comandoSQL + "  " + geraBlocoEndTran_ComRetorno());
        return ds1;
    }

    public DataSet atualizaSprint(int idProjeto, string tituloSprint, string inicio, string termino, string Objetivos, string codigoProjectOwner, string codigoPacoteTrabalho, decimal fatorProdutividade)
    {
        string comandoSQL = string.Format(@"    
        UPDATE {0}.{1}.Projeto 
               SET NomeProjeto = '{3}'
                  ,DescricaoProposta = '{4}'
                  ,CodigoGerenteProjeto = {7}
             WHERE CodigoProjeto = {2}

            UPDATE {0}.{1}.Agil_Iteracao 
               SET InicioPrevisto = CONVERT(DateTime, '{5:dd/MM/yyyy}', 103)
                  ,TerminoPrevisto = CONVERT(DateTime, '{6:dd/MM/yyyy}', 103)
                  ,FatorProdutividade = {9}
                  ,CodigoTarefaItemEAP = {8}
             WHERE CodigoProjetoIteracao = {2} --{6} depois fazer o tratamento destes dois"
            , getDbName(), getDbOwner()
            , idProjeto
            , tituloSprint
            , Objetivos
            , inicio
            , termino
            , codigoProjectOwner
            , (codigoPacoteTrabalho == "-1") ? "null" : codigoPacoteTrabalho
            , fatorProdutividade.ToString().Replace(",", "."));

        DataSet ds = getDataSet(geraBlocoBeginTran() + " " + comandoSQL + " " + geraBlocoEndTran());
        return ds;
    }

    public DataSet excluiSprint(string in_CodigoProjeto, string in_CodigoIteracao, string in_CodigoProjetoPai)
    {
        string comandoSQL = string.Format(@"
        DECLARE @RC int
        DECLARE @in_CodigoProjeto int
        DECLARE @in_CodigoIteracao int
        DECLARE @in_CodigoProjetoPai int

        set @in_CodigoProjeto = {2}
        set @in_CodigoIteracao = {3}
        set @in_CodigoProjetoPai = {4}
        
        EXECUTE @RC = {0}.{1}.[p_Agil_ExcluiIteracao] 
           @in_CodigoProjeto
          ,@in_CodigoIteracao
          ,@in_CodigoProjetoPai ", getDbName(), getDbOwner(), in_CodigoProjeto, in_CodigoIteracao, in_CodigoProjetoPai);
        DataSet ds = getDataSet(geraBlocoBeginTran() + " " + comandoSQL + " " + geraBlocoEndTran());
        return ds;
    }

    #endregion




    #endregion
}
}