<script>
    (function(){
  const KEY = 'qms_theme';
    function apply(mode){
    const root = document.documentElement;
    if(mode === 'dark'){root.classList.add('dark'); }
    else {root.classList.remove('dark'); }
  }
    // init
    apply(localStorage.getItem(KEY) || 'light');

    // expose to window for button onclick
    window.qmsToggleTheme = function(){
    const isDark = document.documentElement.classList.contains('dark');
    const next = isDark ? 'light' : 'dark';
    localStorage.setItem(KEY, next);
    apply(next);
  }
})();
</script>
