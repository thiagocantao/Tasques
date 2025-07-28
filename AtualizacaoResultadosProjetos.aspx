<%@ Page Title="" Language="C#" MasterPageFile="~/novaCdis.master" AutoEventWireup="true" CodeBehind="AtualizacaoResultadosProjetos.aspx.cs" Inherits="BriskPtf._Projetos._Projetos_AtualizacaoResultadosProjetos" %>

<%@ MasterType VirtualPath="~/novaCdis.master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="AreaTrabalho" runat="Server">
    <table>
        <tr>
            <td>
                <dxwgv:ASPxGridView ID="gvDados" runat="server"
                    AutoGenerateColumns="False" ClientInstanceName="gvDados"
                    KeyFieldName="CodigoMetaOperacional" Width="100%" EnableRowsCache="False"
                    EnableViewState="False"
                    OnAutoFilterCellEditorInitialize="gvDados_AutoFilterCellEditorInitialize">
                    <SettingsBehavior AllowFocusedRow="True" AutoExpandAllGroups="True"></SettingsBehavior>

                    <ClientSideEvents CustomButtonClick="function(s, e) {
	OnGridFocusedRowChanged(s, e.visibleIndex);
}"></ClientSideEvents>
                    <Columns>
                        <dxwgv:GridViewCommandColumn ButtonRenderMode="Image" Width="35px" Caption=" " VisibleIndex="0">
                            <CustomButtons>
                                <dxwgv:GridViewCommandColumnCustomButton ID="imgEditar" Text="Atualizar Resultados de Desempenho">
                                    <Image Url="~/imagens/botoes/editarReg02.PNG"></Image>
                                </dxwgv:GridViewCommandColumnCustomButton>
                            </CustomButtons>
                        </dxwgv:GridViewCommandColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeIndicador"
                            Caption="Indicador" VisibleIndex="2">
                            <Settings AllowAutoFilter="True" AutoFilterCondition="Contains" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoIndicador" Visible="False"
                            VisibleIndex="7">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CasasDecimais" Visible="False"
                            VisibleIndex="7">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeUsuario"
                            VisibleIndex="3" Caption="Responsável pelo Indicador" Width="210px">
                            <Settings AutoFilterCondition="Contains" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="NomeProjeto" Caption="Projeto"
                            VisibleIndex="4" Width="350px">
                            <Settings AutoFilterCondition="Contains" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataDateColumn Caption="Última Atualização"
                            FieldName="UltimaAtualizacao" VisibleIndex="5" Width="125px">
                            <PropertiesDateEdit DisplayFormatString="dd/MM/yyyy">
                            </PropertiesDateEdit>
                            <Settings ShowFilterRowMenu="True" />
                            <HeaderStyle HorizontalAlign="Center" />
                            <CellStyle HorizontalAlign="Center">
                            </CellStyle>
                        </dxwgv:GridViewDataDateColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="SiglaUnidadeMedida"
                            ShowInCustomizationForm="True" Visible="False" VisibleIndex="7">
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn Caption="Tipo de Indicador"
                            FieldName="DescTipoIndicador" GroupIndex="0" Name="TipoIndicador" SortIndex="0"
                            SortOrder="Ascending" VisibleIndex="1" Width="150px">
                            <Settings AllowAutoFilter="True" AllowHeaderFilter="True" />
                        </dxwgv:GridViewDataTextColumn>
                        <dxwgv:GridViewDataTextColumn FieldName="CodigoProjeto" Visible="False"
                            VisibleIndex="6">
                        </dxwgv:GridViewDataTextColumn>
                    </Columns>

                    <SettingsPager PageSize="50">
                    </SettingsPager>

                    <Settings VerticalScrollBarMode="Visible" ShowFilterRow="True" ShowGroupPanel="True"
                        ShowHeaderFilterBlankItems="False"></Settings>
                    <StylesEditors>
                        <Calendar>
                        </Calendar>
                    </StylesEditors>
                </dxwgv:ASPxGridView>
            </td>
        </tr>
    </table>
</asp:Content>

