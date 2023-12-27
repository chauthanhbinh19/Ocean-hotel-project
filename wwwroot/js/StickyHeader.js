window.onscroll = function() {myFunction()};
var header = document.getElementById("header");
var sticky = header.offsetTop;
function myFunction() {
  if (window.pageYOffset > sticky) {
    header.classList.add("change-bg");
    header.classList.remove("change-bg1");
  } else {
    header.classList.remove("change-bg");
    header.classList.add("change-bg1");
  }
}
$(document).ready(function(){
  $(".remove-service").on('click',function() {
    var id=$(this).val()
    console.log("hi")
    // $('#service-name-'+id).removeClass('added');
    // $('#delete-service-'+id).remove();
  })
})
