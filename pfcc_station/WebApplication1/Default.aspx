<%@ Page Title="PF-CC機器數據轉化" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header runat="server">
    <script type="text/javascript">
        function hideElements() {
            var elements = document.getElementsByClassName('hideable');
            for (var i = 0; i < elements.length; i++) {
                elements[i].style.display = 'none';
            }
        }

        function showElements() {
            var elements = document.getElementsByClassName('hideable');
            for (var i = 0; i < elements.length; i++) {
                elements[i].style.display = '';
            }
        }
    </script>
    </header>

   
    <div runat="server" >
        
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="分析PF&amp;CC"  CssClass="hideable"/>
            <!--<asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="分析CC" />-->
            <asp:Button ID="Button3" runat="server" OnClick="Button3_Click" Text="取得檔案路徑" CssClass="hideable"/>
            <asp:Button ID="Btn_Auto" runat="server" OnClick="Btn_Auto_Click" Text="全自動解析" />
       <br />處理的檔案名稱(暫時):<asp:TextBox ID="TextBox1" runat="server" Width="1321px"></asp:TextBox>
        
        <p>
            <asp:Label ID="Label_Auto" runat="server" Text="自動等待執行中"></asp:Label>
            </p>
         <div>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Timer ID="Timer1" runat="server" OnTick="Timer1_Tick"></asp:Timer>
                    <asp:Label ID="Label1" runat="server" Text="等待(秒):"></asp:Label>
                    <br/><br/>
                    <asp:Label ID="Label2" runat="server" Text="處理狀態:"></asp:Label>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <h3>
            <asp:Label ID="LResult" runat="server" Text=""></asp:Label>
        </h3>
        <p>
            <asp:GridView ID="GridView1" runat="server" Height="35px" Width="162px" Visible="False">
            </asp:GridView>
            來源檔<asp:GridView ID="GridView2" runat="server" Visible="False">
            </asp:GridView>
        </p>
        <p>
            目的檔</p>

       <p>
            轉換過程</p>
            <asp:GridView ID="csvview" runat="server"  Height="230px" Width="404px" Visible="true">
            </asp:GridView>
    </div>
      
</asp:Content>
