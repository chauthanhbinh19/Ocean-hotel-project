// function getDate(){
//     var ngay=13;
//     var thang=10;
//     var nam=2023;
//     var thu=["thu 5", "thu 6", "thu 7", "CN","thu 2","thu 3","thu 4"];
//     s=ngay;
//     // 1/1/1970 thứ năm
//     for(i=1;i<thang;i++){
//         switch(i) {
//             case 1:
//             case 3:
//             case 5:
//             case 7:
//             case 8:
//             case 10:
//             case 12:
//                 s=s+31;
//                 break;
//             case 4:
//             case 6:
//             case 9:
//             case 11:
//                 s=s+30
//                 break;
//             case 2:
//                 if((nam%4==0)&&(nam%100!=0)||(nam%400==0))
//                     s=s+29;
//                 else 
//                     s=s+28;
//                 break;
//         }
//     }
//     for(i=1970;i< nam;i++){
//         if((i%4==0)&&(i%100!=0)||(i%400==0))
//             s=s+366;
//         else
//             s=s+365;
//     }
//     s=s-1;
//     var du=s%7;
//     console.log(thu[du]);
// }
$('#button1').click(function(){
    alert("Hello!");
});