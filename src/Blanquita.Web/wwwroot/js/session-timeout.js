window.sessionTimeout = {
    timer: null,
    dotNetHelper: null,
    timeoutInMilliseconds: 0,
    initialize: function (helper, timeoutInMinutes) {
        this.dotNetHelper = helper;
        this.timeoutInMilliseconds = timeoutInMinutes * 60 * 1000;

        // Register events
        document.onmousemove = this.resetTimer.bind(this);
        document.onkeypress = this.resetTimer.bind(this);
        document.onclick = this.resetTimer.bind(this);
        document.onscroll = this.resetTimer.bind(this);
        document.ontouchstart = this.resetTimer.bind(this);

        this.startTimer();
    },
    startTimer: function () {
        if (this.timer) {
            clearTimeout(this.timer);
        }
        this.timer = setTimeout(this.onTimeout.bind(this), this.timeoutInMilliseconds);
    },
    resetTimer: function () {
        if (this.timer) {
            clearTimeout(this.timer);
        }
        this.startTimer();
    },
    onTimeout: function () {
        if (this.dotNetHelper) {
            this.dotNetHelper.invokeMethodAsync('OnSessionTimeout');
        }
    },
    cleanup: function () {
        document.onmousemove = null;
        document.onkeypress = null;
        document.onclick = null;
        document.onscroll = null;
        document.ontouchstart = null;
        if (this.timer) {
            clearTimeout(this.timer);
        }
    }
};
