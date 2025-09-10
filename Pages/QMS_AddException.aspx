<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_AddException.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_AddException"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Add Exception" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
  <title>Add New Quality Exception</title>
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />
  <style>
    .qms-card{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
    .qms-panel{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
    .muted{color:var(--muted-foreground)}
    .thumb{width:88px;height:88px;object-fit:cover;border-radius:10px;border:1px solid var(--border)}
    .thumb-wrap{position:relative;display:inline-block;margin:.25rem}
    .thumb-wrap button{position:absolute;right:-6px;top:-6px}
    .req:after{content:" *";color:#e03131}
  </style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">

  <div class="d-flex align-items-center justify-content-between mb-3">
    <div class="d-flex align-items-center gap-2">
      <a class="btn btn-outline-secondary" href="<%= ResolveUrl("~/Pages/QMS_QualityIssueSummary.aspx") %>">
        <i class="bi bi-arrow-left me-1"></i> Back to Summary
      </a>
      <h2 class="fw-bold m-0">Add New Quality Exception</h2>
    </div>
    <div class="d-flex gap-2">
      <asp:Button ID="btnReset" runat="server" CssClass="btn btn-outline-secondary" Text="Reset" OnClick="btnReset_Click" />
      <asp:Button ID="btnSave" runat="server" CssClass="btn btn-dark" Text="Save Exception" OnClick="btnSave_Click" />
    </div>
  </div>

  <!-- Success card -->
  <asp:Panel ID="pnSuccess" runat="server" Visible="false" CssClass="qms-card p-4 mb-4">
    <div class="text-center">
      <i class="bi bi-check-circle-fill fs-1 text-success"></i>
      <h4 class="mt-2 mb-1">Exception Submitted Successfully!</h4>
      <p class="muted mb-3">Your quality exception has been recorded and will be reviewed by the responsible department.</p>
      <a class="btn btn-dark me-2" href="<%= ResolveUrl("~/Pages/QMS_AddException.aspx") %>">Add Another</a>
      <a class="btn btn-outline-secondary" href="<%= ResolveUrl("~/Pages/QMS_QualityIssueSummary.aspx") %>">Go to Summary</a>
    </div>
  </asp:Panel>

  <asp:ValidationSummary ID="valSummary" runat="server" CssClass="alert alert-danger"
    HeaderText="Please correct the highlighted fields:" DisplayMode="BulletList" />

  <!-- Section: Exception Information -->
  <div class="qms-panel p-3 mb-3">
    <h5 class="fw-semibold mb-3">Exception Information</h5>
    <div class="row g-3">
      <div class="col-12 col-md-3">
        <label class="form-label req">Production Year</label>
        <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlYear" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Production year is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Production Week</label>
        <asp:DropDownList ID="ddlWeek" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlWeek" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Production week is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Date</label>
        <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDate"
          CssClass="text-danger small" ErrorMessage="Date is required" />
      </div>
    </div>
  </div>

  <!-- Section: Production / Defect -->
  <div class="qms-panel p-3 mb-3">
    <h5 class="fw-semibold mb-3">Production &amp; Defect</h5>
    <div class="row g-3">
      <div class="col-12 col-md-3">
        <label class="form-label req">Production Unit</label>
        <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlUnit" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Production unit is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Line</label>
        <asp:DropDownList ID="ddlLine" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlLine" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Line is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Defect Type</label>
        <asp:DropDownList ID="ddlDefect" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlDefect" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Defect type is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Product</label>
        <asp:DropDownList ID="ddlProduct" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlProduct" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Product is required" />
      </div>
      <div class="col-12">
        <label class="form-label req">Description</label>
        <asp:TextBox ID="txtDesc" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
          placeholder="Describe the quality exception..." />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDesc"
          CssClass="text-danger small" ErrorMessage="Description is required" />
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label req">Quantity</label>
        <asp:TextBox ID="txtQty" runat="server" CssClass="form-control" TextMode="Number" min="0" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtQty"
          CssClass="text-danger small" ErrorMessage="Quantity is required" />
      </div>
    </div>
  </div>

  <!-- Section: Responsibility -->
  <div class="qms-panel p-3 mb-3">
    <h5 class="fw-semibold mb-3">Responsibility</h5>
    <div class="row g-3">
      <div class="col-12 col-md-4">
        <label class="form-label req">Responsible Department</label>
        <asp:DropDownList ID="ddlRespDept" runat="server" CssClass="form-select" />
        <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlRespDept" InitialValue=""
          CssClass="text-danger small" ErrorMessage="Responsible department is required" />
      </div>
      <div class="col-12 col-md-8">
        <label class="form-label">Responsible Action</label>
        <asp:TextBox ID="txtRespAction" runat="server" CssClass="form-control" placeholder="Action taken..." />
      </div>
      <div class="col-12">
        <label class="form-label">Remarks</label>
        <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
      </div>
    </div>
  </div>

  <!-- Section: Images -->
  <div class="qms-panel p-3">
    <h5 class="fw-semibold mb-3">Defect Images</h5>
    <div class="mb-2 muted">Allowed: images only. You can select multiple files.</div>
    <asp:FileUpload ID="fuImages" runat="server" CssClass="form-control" />
    <small class="muted">Tip: Hold Ctrl/Shift to select multiple files</small>

    <div id="thumbs" class="mt-3"></div>

    <div class="d-flex gap-2 mt-3">
      <button type="button" class="btn btn-light" id="btnClearImages"><i class="bi bi-x-circle"></i> Clear Images</button>
    </div>
  </div>

</asp:Content>

<asp:Content ID="Scripts1" ContentPlaceHolderID="BodyScripts" runat="server">
<script>
  // allow multiple
  document.addEventListener('DOMContentLoaded', function() {
    const fu = document.getElementById('<%= fuImages.ClientID %>');
    fu.setAttribute('multiple','multiple');
    fu.setAttribute('accept','image/*');

    const thumbs = document.getElementById('thumbs');
    const clearBtn = document.getElementById('btnClearImages');

    fu.addEventListener('change', function(){
      thumbs.innerHTML = '';
      [...fu.files].forEach((file, idx) => {
        if(!file.type.startsWith('image/')) return;
        const reader = new FileReader();
        reader.onload = e => {
          const wrap = document.createElement('div');
          wrap.className = 'thumb-wrap';
          wrap.innerHTML =
            '<img class="thumb" src="'+ e.target.result +'" alt="img'+idx+'"/>' +
            '<button type="button" class="btn btn-sm btn-light border"><i class="bi bi-x"></i></button>';
          wrap.querySelector('button').onclick = () => { wrap.remove(); /* visual only */ };
          thumbs.appendChild(wrap);
        };
        reader.readAsDataURL(file);
      });
    });

    clearBtn.addEventListener('click', function(){
      fu.value = ''; thumbs.innerHTML = '';
    });
  });
</script>
</asp:Content>
