$(document).ready(function() {
    $(".image-container").owlCarousel({
      nav:true,
      margin:20,
      loop:true,
      autoWidth:true,
      items:4,
      autoplay:true,
      autoplayTimeout:2000,
      autoplayHoverPause:true,
      smartSpeed:1000,
      navText : ["<i class='fa fa-chevron-left'></i>","<i class='fa fa-chevron-right'></i>"]
    });
  });