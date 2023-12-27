const signIn_btn =document.querySelector("#sign-in-btn");
const signUp_btn =document.querySelector("#sign-up-btn");
const container = document.querySelector(".container");
signUp_btn.addEventListener('click',()=>{
    container.classList.add("sign-up-mode")
})
signIn_btn.addEventListener('click',()=>{
    container.classList.remove("sign-up-mode")
})
