<%@ Page Title="Log out" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="Logout.aspx.vb" Inherits="LogoutPage" %>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
<div class="form-panel narrow"><h1>Log out?</h1><p>Your cart will be saved for your next visit.</p><asp:Button ID="SaveButton" runat="server" Text="Confirm log out" CssClass="btn btn-primary" OnClick="LogoutClick" CausesValidation="true" /></div>
</asp:Content>
