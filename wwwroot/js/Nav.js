$(document).ready(function(){
    $(".hassub").click(function(){
       $(this).next(".sub-menu").slideToggle();
       
    });
    $(".has-moresub").click(function(){
       $(this).next(".more-menu").slideToggle();
  });
  });