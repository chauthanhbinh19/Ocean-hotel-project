document.getElementById('next').onclick = function() {
    let list = document.querySelectorAll('.bg-image');
    document.getElementById('slide').appendChild(list[0]);
}
document.getElementById('prev').onclick = function() {
    let list = document.querySelectorAll('.bg-image');
    document.getElementById('slide').prepend(list[list.length-1]);
}