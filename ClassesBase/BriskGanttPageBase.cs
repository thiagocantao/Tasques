
using System.Web;
namespace BriskPtf
{

    /// <summary>
    /// Summary description for GanttPageBase
    /// </summary>
    public class BriskGanttPageBase : BasePageBrisk
    {
        #region Columns
        #region Pt_BR
        private readonly string columnPt_BR = @"[
                                                      {
                                                        'type': 'wbs',
                                                        'text': '#',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'type': 'name',
                                                        'text': 'Tarefa',
                                                        'width': 250
                                                      },
                                                      {
                                                        'type': 'startdate',
                                                        'text': 'Início LB',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'type': 'enddate',
                                                        'text': 'Término LB',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'previsto',
                                                        'type': 'column',
                                                        'text': 'Previsto',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'type': 'percentdone',
                                                        'text': 'Realizado',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'pesoLb',
                                                        'text': 'Peso LB',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'peso',
                                                        'text': '% Peso',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'duracao',
                                                        'text': 'Duração (d)',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'trabalho',
                                                        'text': 'Trabalho (h)',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'inicio',
                                                        'text': 'Início',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'termino',
                                                        'text': 'Término',
                                                        'flex': 1
                                                      },
                                                      {
                                                        'field': 'termino',
                                                        'text': 'Término Real',
                                                        'flex': 1
                                                      }
                                                    ]";
        #endregion

        #region En
        private readonly string columnEn = @"[
                                                    {
                                                    'type': 'wbs',
                                                    'text': '#',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'type': 'name',
                                                    'text': 'Task',
                                                    'width': 250
                                                    },
                                                    {
                                                    'type': 'startdate',
                                                    'text': 'LB Start',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'type': 'enddate',
                                                    'text': 'LB End',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'previsto',
                                                    'type': 'column',
                                                    'text': 'Predicted',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'type': 'percentdone',
                                                    'text': 'Realized',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'pesoLb',
                                                    'text': 'LB Weight',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'peso',
                                                    'text': '% Weight',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'duracao',
                                                    'text': 'Duration (d)',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'trabalho',
                                                    'text': 'Job (h)',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'inicio',
                                                    'text': 'Start',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'termino',
                                                    'text': 'End',
                                                    'flex': 1
                                                    },
                                                    {
                                                    'field': 'termino',
                                                    'text': 'Real End',
                                                    'flex': 1
                                                    }
                                                ]";
        #endregion
        #endregion
        public string GetLangPage()
        {
            string currentlang = GetCurrentLangCode();
            //Caso seja incluído ao sistema Brisk novas linguagens, 
            //será necessário criar uma nova tradução do gráfico Gantt pra linguagem incluída
            //e o código abaixo deverá ser refatorado.        
            return currentlang.Equals("pt-BR") ? "Pt_BR" : "En";
        }

        /// <summary>
        /// Buscar o Id em string do IDProjeto para o gráfico
        /// </summary>        
        public string GetIdProjeto(HttpRequest request)
        {
            return request.QueryString["IDProjeto"] == null ? "0" : request.QueryString["IDProjeto"].ToString();
        }

        /// <summary>
        /// Buscar as colunas do gráfico gantt de acordo com a língua
        /// </summary>        
        public string GetStrJsonColumnDetailGantt()
        {
            switch (GetCurrentLangCode())
            {
                case "pt-BR":
                    return columnPt_BR;
                case "en-US":
                    return columnEn;
                default: return "";
            }
        }
    }


}
