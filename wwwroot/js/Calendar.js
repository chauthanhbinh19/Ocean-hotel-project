
$(document).ready(function() {
    let checkday=1;
    var firstClickedIndex=0;
    var currentClickedIndex=0;
    var curentDate=new Date();
    var currentYear=curentDate.getFullYear();
    var currentMonth=curentDate.getMonth()+1;
    //Lấy tên tháng
    var date_month=new Date(currentYear,currentMonth-1);
    var date_month1=new Date(currentYear,currentMonth);
    var month=date_month.toLocaleString('en-US',{month:'long'});
    var month1=date_month1.toLocaleString('en-US',{month:'long'});
    $('.month-name1').text(month +" "+currentYear);
    $('.month-name2').text(month1 +" "+currentYear);
    //Tạo lịch
    var days1=getAllDaysInMonth(currentYear,currentMonth);
    var i=0;
    getDaysData(i,days1);
    var days2=getAllDaysInMonth(currentYear,currentMonth+1);
    i=0;
    getDaysData1(i,days2);
    $('.calendar-table-box .calendar-days').click(function(e){
        if(checkday==3){
            $('.calendar-table-box .calendar-days').each(function(){
                if($(this).hasClass('calendar-days-active')){
                    $(this).removeClass('calendar-days-active');
                    $(this).find('.day-in').css('display', 'none');
                    $(this).find('.day-out').css('display', 'none');
                    $(this).find('.day-price').css('display', 'block');
                }
            });
            checkday=1;
        }
        if(checkday<=2){
            if($(this).hasClass('calendar-days-active')){
                $(this).removeClass('calendar-days-active');
                $(this).find('.day-in').css('display', 'none');
                $(this).find('.day-out').css('display', 'none');
                $(this).find('.day-price').css('display', 'block');
            }else{
                if(checkday ==1){
                    firstClickedIndex=$('.calendar-table-box .calendar-days').index(this);
                    $(this).addClass('calendar-days-active');
                    $(this).find('.day-in').css('display', 'block');
                    $(this).find('.day-price').css('display', 'none');
                    checkday=checkday+1;
                }else if(checkday==2){
                    currentClickedIndex=$('.calendar-table-box .calendar-days').index(this);
                    $(this).addClass('calendar-days-active');
                    if(currentClickedIndex<firstClickedIndex){
                        var td = $('.calendar-table-box .calendar-days').eq(firstClickedIndex);
                        $(this).find('.day-in').css('display', 'block');
                        $(this).find('.day-price').css('display', 'none');
                        $(td).find('.day-out').css('display', 'block');
                        $(td).find('.day-in').css('display', 'none');
                    }else{
                        $(this).find('.day-out').css('display', 'block');
                        $(this).find('.day-price').css('display', 'none');
                    }
                    var start= Math.min(firstClickedIndex,currentClickedIndex);
                    var end=Math.max(firstClickedIndex,currentClickedIndex);
                    $('.calendar-table-box .calendar-days').slice(start,end+1).filter(function(){return !$(this).hasClass('empty')}).addClass('calendar-days-active');
                    $('.calendar-table-box .calendar-days').slice(start,end+1).filter(function(){return !$(this).hasClass('empty')}).find('.day-price').css('display', 'none');
                    checkday=checkday+1;
                }
            }
        }
    });
    //Button left
    $('#left').click(function() {
        var j=0;
        currentMonth=currentMonth-1;
        if(currentMonth==0) {
            currentMonth=12
            currentYear=currentYear-1;
        }
        date_month=new Date(currentYear,currentMonth-1);
        date_month1=new Date(currentYear,currentMonth);
        month=date_month.toLocaleString('en-US',{month:'long'});
        month1=date_month1.toLocaleString('en-US',{month:'long'});
        $('.month-name1').text(month +" "+currentYear);
        $('.month-name2').text(month1 +" "+currentYear);
        var days3=getAllDaysInMonth(currentYear,currentMonth);
        var days4=getAllDaysInMonth(currentYear,currentMonth+1);
        getDaysData(j, days3);
        j=0;
        getDaysData1(j, days4);
            //kiểm tra valid-day và empty
        $('.calendar-table-box .calendar-body tr td').each(function(){
            if($(this).hasClass('valid-day')){
                $(this).find('.day-price').css('display', 'block');
            }
            if($(this).hasClass('empty')){
                $(this).find('.day-price').css('display', 'none');
            }
        });
    });
    //Button right
    $('#right').click(function() {
        var j=0;
        currentMonth=currentMonth+1;
        if(currentMonth==12) {
            currentMonth=1;
            currentYear=currentYear+1;
        }
        date_month=new Date(currentYear,currentMonth-1);
        date_month1=new Date(currentYear,currentMonth);
        month=date_month.toLocaleString('en-US',{month:'long'});
        month1=date_month1.toLocaleString('en-US',{month:'long'});
        $('.month-name1').text(month +" "+currentYear);
        $('.month-name2').text(month1 +" "+currentYear);
        var days3=getAllDaysInMonth(currentYear,currentMonth);
        var days4=getAllDaysInMonth(currentYear,currentMonth+1);
        getDaysData(j, days3);
        j=0;
        getDaysData1(j, days4);
            //kiểm tra valid-day và empty
        $('.calendar-table-box .calendar-body tr td').each(function(){
            if($(this).hasClass('valid-day')){
                $(this).find('.day-price').css('display', 'block');
            }
            if($(this).hasClass('empty')){
                $(this).find('.day-price').css('display', 'none');
            }
        });
    });
});
function getAllDaysInMonth(year,month){
    var daysInMonth= new Date(year,month,0).getDate();
    var days=[];
    for(var day=1;day<=daysInMonth; day++){
        var currentDay=new Date(year,month-1,day).getDay();
        days.push({day: day, weekday: currentDay });
    }
    return days;
}
function getDaysData(i,days1){
    $('.month-1 .calendar-body .calendar-week td').each(function(index){
        var currentWeekday=$(this).closest("table").find("thead th").eq($(this).index()).text();
        // console.log(currentWeekday); 
        if(i<days1.length){
            if(currentWeekday=="Mo"){
                if(days1[i].weekday==1){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Tu"){
                if(days1[i].weekday==2){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="We"){
                if(days1[i].weekday==3){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Th"){
                if(days1[i].weekday==4){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Fr"){
                if(days1[i].weekday==5){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Sa"){
                if(days1[i].weekday==6){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Su"){
                if(days1[i].weekday==0){
                    $(this).find('.day-number').text(days1[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }
        }else{
            $(this).find('.day-number').text("");
            $(this).find('.day-price').css('display', 'none');
            if($(this).hasClass('valid-day')){
                $(this).removeClass('valid-day');
            }
            $(this).addClass('empty');
        } 
    });
}
function getDaysData1(i,days2){
    $('.month-2 .calendar-table-box .calendar-body .calendar-week td').each(function(index){
        var currentWeekday=$(this).closest("table").find("thead th").eq($(this).index()).text();
        // console.log(currentWeekday); 
        if(i<days2.length){
            if(currentWeekday=="Mo"){
                if(days2[i].weekday==1){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Tu"){
                if(days2[i].weekday==2){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="We"){
                if(days2[i].weekday==3){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Th"){
                if(days2[i].weekday==4){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Fr"){
                if(days2[i].weekday==5){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Sa"){
                if(days2[i].weekday==6){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }else if(currentWeekday=="Su"){
                if(days2[i].weekday==0){
                    $(this).find('.day-number').text(days2[i].day);
                    if($(this).hasClass('empty')){
                        $(this).removeClass('empty');
                    }
                    $(this).addClass('valid-day');
                    i=i+1;
                }else{
                    $(this).find('.day-number').text("");
                    $(this).find('.day-price').css('display', 'none');
                    if($(this).hasClass('valid-day')){
                        $(this).removeClass('valid-day');
                    }
                    $(this).addClass('empty');
                }
            }
        }else{
            $(this).find('.day-number').text("");
            $(this).find('.day-price').css('display', 'none');
            if($(this).hasClass('valid-day')){
                $(this).removeClass('valid-day');
            }
            $(this).addClass('empty');
        } 
    });
}
