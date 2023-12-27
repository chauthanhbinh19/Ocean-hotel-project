$(document).ready(function() {
    $('.toggle').on('click',function(){
        $('.navigation').toggleClass('active');
        $('.main').toggleClass('active');
        $('.top-bar').toggleClass('active');
        console.log("hi");
    })
    $('.nav-ul>li >a').click(function(){
        $('.nav-ul>li >a').each(function(){
            $(this).find(".tittle .fas").removeClass("fa-caret-down");
            $(this).find(".tittle .fas").addClass("fa-caret-right");

        });
        if($(this).hasClass('hovered')){
            $(this).find(".tittle .fas").addClass("fa-caret-right");
            $(this).find(".tittle .fas").removeClass("fa-caret-down");
            $(this).removeClass('hovered').addClass('unactive');
        }else{
            $('.nav-ul>li >a').removeClass('hovered');
            $(this).addClass('hovered');
            $(this).find(".tittle .fas").removeClass("fa-caret-right");
            $(this).find(".tittle .fas").addClass("fa-caret-down");
        }      
    });
    $('.nav-sub-ul > li').click(function(){
        $('.nav-sub-ul > li').each(function(){
            $(this).removeClass('active');
        });
        $(this).addClass('active');
        $(this).removeClass('unactive');
    });
});
$(document).ready(function(){
    $('.nav-ul> li >a').each(function(){
        $(this).addClass('unactive');
    });
    $('.nav-ul> li >a').each(function(){
        if($(this).hasClass('hovered')){
            $(this).removeClass('unactive');
            $(this).find(".tittle .fas").removeClass("fa-caret-right");
            $(this).find(".tittle .fas").addClass("fa-caret-down");
        }else{
            $(this).find(".tittle .fas").addClass("fa-caret-right");
            $(this).find(".tittle .fas").removeClass("fa-caret-down");
        }
    });
});
$(document).ready(function() {
    $('.nav-guest-ul .nav-item a').click(function(){
        $('.nav-guest-ul .nav-item a').removeClass('active');
        $(this).addClass('active');
        var textValue=$(this).text();
        $('.guest-list > div').each(function(){
            $(this).removeClass('active-table');
        });
        if(textValue=="All Bookings"){
            $('.guest-list .all').addClass('active-table');
        }else if(textValue=="Pending"){
            $('.guest-list .pending').addClass('active-table');
        }else if(textValue=="Booked"){
            $('.guest-list .booked').addClass('active-table');
        }else if(textValue=="Refund"){
            $('.guest-list .refund').addClass('active-table');
        }else if(textValue=="Canceled"){
            $('.guest-list .canceled').addClass('active-table');
        }
    });
    $('.nav-room-ul .nav-item a').click(function(){
        $('.nav-room-ul .nav-item a').removeClass('active');
        $(this).addClass('active');
        var textValue=$(this).text();
        $('.room-list > div').each(function(){
            $(this).removeClass('active-table');
        });
        if(textValue=="All Room"){
            $('.room-list .all').addClass('active-table');
        }else if(textValue=="Standard"){
            $('.room-list .standard').addClass('active-table');
        }else if(textValue=="Superior"){
            $('.room-list .superior').addClass('active-table');
        }else if(textValue=="Deluxe"){
            $('.room-list .deluxe').addClass('active-table');
        }else if(textValue=="Suite"){
            $('.room-list .suite').addClass('active-table');
        }
        $('.employee-list > div').each(function(){
            $(this).removeClass('active-table');
        });
        if(textValue=="All Employees"){
            $('.employee-list .all').addClass('active-table');
        }else if(textValue=="Active"){
            $('.employee-list .active').addClass('active-table');
        }else if(textValue=="Inactive"){
            $('.employee-list .inactive').addClass('active-table');
        }
        $('.customer-list > div').each(function(){
            $(this).removeClass('active-table');
        });
        if(textValue=="All Customers"){
            $('.customer-list .all').addClass('active-table');
        }else if(textValue=="Active"){
            $('.customer-list .active').addClass('active-table');
        }else if(textValue=="Inactive"){
            $('.customer-list .inactive').addClass('active-table');
        }
    });
    $(".btn3").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn4").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn5").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn6").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn7").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn9").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn10").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn11").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn12").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn13").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $(".btn16").click(function() {
        $(".nav-sub-ul").not($(this).next()).hide();
        $(this).next().toggle();
    });
    $('.edit .btn-change').click(function(){
        $(this).find('.drop-down').toggleClass('drop-down-active');
    });
});
$(document).ready(function(){
    var darkmode= localStorage.getItem("darkmode");
    // if(darkmode ==1){
    //     document.documentElement.style.setProperty('--bg-blue','rgb(0, 106, 188)');
    //     document.documentElement.style.setProperty('--cl-white2','#f5f5f5');
    //     document.documentElement.style.setProperty('--cl-white','white');
    //     $('#change-dark-mode i').removeClass("fa-solid fa-moon");
    //     $('#change-dark-mode i').addClass("fa-solid fa-sun");
    //     localStorage.setItem("darkmode",0);
    // }
    if(darkmode==1){
        document.documentElement.style.setProperty('--bg-blue','#212130');
        document.documentElement.style.setProperty('--cl-white2','#171622');
        document.documentElement.style.setProperty('--cl-white','#171622');
        document.documentElement.style.setProperty('--cl-black-white','white');
        document.documentElement.style.setProperty('--cl-white-black','#212130');
        document.documentElement.style.setProperty('--box-shadow-cl5','rgba(255, 255, 255, 0.3)');
        $('#change-dark-mode i').removeClass("fa-solid fa-sun");
        $('#change-dark-mode i').addClass("fa-solid fa-moon");
        localStorage.setItem("darkmode",1);
    }
    $('#change-dark-mode').on("click", function(){
        var darkmode= localStorage.getItem("darkmode");
        if(darkmode ==1){
            document.documentElement.style.setProperty('--bg-blue','white');
            document.documentElement.style.setProperty('--cl-white2','#f5f5f5');
            document.documentElement.style.setProperty('--cl-white','white');
            document.documentElement.style.setProperty('--cl-black-white','black');
            document.documentElement.style.setProperty('--cl-white-black','white');
            document.documentElement.style.setProperty('--box-shadow-cl5','rgba(0, 0, 0, 0.3)');
            $('#change-dark-mode i').removeClass("fa-solid fa-moon");
            $('#change-dark-mode i').addClass("fa-solid fa-sun");
            localStorage.setItem("darkmode",0);
        }else{
            document.documentElement.style.setProperty('--bg-blue','#212130');
            document.documentElement.style.setProperty('--cl-white2','#171622');
            document.documentElement.style.setProperty('--cl-white','#171622');
            document.documentElement.style.setProperty('--cl-black-white','white');
            document.documentElement.style.setProperty('--cl-white-black','#212130');
            document.documentElement.style.setProperty('--box-shadow-cl5','rgba(255, 255, 255, 0.3)');
            $('#change-dark-mode i').removeClass("fa-solid fa-sun");
            $('#change-dark-mode i').addClass("fa-solid fa-moon");
            localStorage.setItem("darkmode",1);
        }
    })
});
$(document).ready(function(){
    $('.bottom-number li').each(function(){
        if($(this).hasClass('PagedList-skipToLast')){
            if($(this).hasClass('next-button')){
                $('.bottom-number li:nth-last-child(3)').addClass('border-ntn2');
                // $('.next-button').css('background','white');
            }else{
                $('.bottom-number li:nth-last-child(3)').addClass('border-ntn2');
                // $('.next-button').css('background','white');
            }
        }else{
            if($(this).hasClass('next-button')){
                $('.bottom-number li:nth-last-child(2)').addClass('border-ntn2');
                // $('.next-button').css('background','white');
            }
        }
        if($(this).hasClass('PagedList-skipToFirst')){
            if($(this).hasClass('previous-button')){
                $('.bottom-number li:nth-child(3)').addClass('border-ntn3');
                // $('.previous-button').css('background','white');
            }else{
                $('.bottom-number li:nth-child(3)').addClass('border-ntn3');
                // $('.previous-button').css('background','white');
            }
        }else{
            if($(this).hasClass('previous-button')){
                $('.bottom-number li:nth-child(2)').addClass('border-ntn3');
                // $('.previous-button').css('background','white');
            }
        }
        if($(this).hasClass('PagedList-skipToFirst')){
            $('.PagedList-skipToFirst').addClass('first-button');
            $('.PagedList-skipToFirst').addClass('horizontal-paging');
        }
        if($(this).hasClass('PagedList-skipToLast')){
            $('.PagedList-skipToLast').addClass('last-button');
            $('.PagedList-skipToLast').addClass('horizontal-paging');
        }
    });
});
$(document).ready(function() {
    var images = $(".image-container .box-img-1");
    var currentIndex = 0;
  
    setInterval(function() {
      images.eq(currentIndex).removeClass("active");
      currentIndex = (currentIndex + 1) % images.length;
      images.eq(currentIndex).addClass("active");
    }, 1500);
  });




