<template>
    <a :class="{active : currentPath == '/login'}" style="float: right;" href="/login">Login</a>
    <a :class="{active : currentPath == '/register'}" style="float: right;" href="/register">Register</a>
</template>

<script>
export default{
    mounted(){
        this.currentPath = window.location.pathname;
    },
    data() {
        return {
            currentPath: '',
        };
    }
}
</script>