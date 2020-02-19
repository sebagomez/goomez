"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
require("../goomez.css");
var React = require("react");
var ResultFile_1 = require("./ResultFile");
var Search = /** @class */ (function (_super) {
    __extends(Search, _super);
    function Search() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Search.prototype.search = function (pattern) {
        var _this = this;
        this.setState({ files: [], loading: true });
        var t0 = performance.now();
        fetch('api/Search/Search?pattern=' + pattern)
            .then(function (response) { return response.json(); })
            .then(function (data) {
            var delta = performance.now() - t0;
            _this.setState({ files: data, pattern: pattern, loading: false, milliseconds: delta });
        })
            .catch(function (error) {
            _this.setState({ files: [], pattern: pattern, loading: false });
            console.error(error);
        });
    };
    Search.prototype.updateInputValue = function (evt) {
        this.setState({ pattern: evt.target.value });
    };
    Search.prototype.keyUpHandler = function (evt) {
        if (evt.keyCode && evt.keyCode === 13)
            this.goSearch();
    };
    Search.prototype.componentDidMount = function () {
        var pattern = decodeURIComponent(this.props.location.search.substr(3));
        this.search(pattern);
    };
    Search.prototype.goHome = function () {
        this.props.history.push({ pathname: '/' });
    };
    Search.prototype.goSearch = function () {
        this.props.history.push({ pathname: '/search', search: '?q=' + this.state.pattern });
        this.search(this.state.pattern);
    };
    Search.prototype.render = function () {
        var _this = this;
        var pattern = decodeURIComponent(this.props.location.search.substr(3));
        var contents = !this.state || this.state.loading
            ? React.createElement("div", { className: "resultBody smallCount" },
                "Loading resutls for ",
                pattern,
                "...")
            : this.rednderResults(this.state.files);
        return React.createElement("div", null,
            React.createElement("div", { className: "resultHeader" },
                React.createElement("div", { className: "smallLogo", title: "Go home", onClick: function () { return _this.goHome(); } },
                    React.createElement("span", { className: "blue" }, "G"),
                    React.createElement("span", { className: "red" }, "o"),
                    React.createElement("span", { className: "yellow" }, "o"),
                    React.createElement("span", { className: "blue" }, "m"),
                    React.createElement("span", { className: "green" }, "e"),
                    React.createElement("span", { className: "red z" }, "z")),
                React.createElement("div", { className: "inputDiv" },
                    React.createElement("input", { className: "bigInput", name: "inputPattern", value: this.state ? this.state.pattern : '', onChange: function (evt) { return _this.updateInputValue(evt); }, onKeyUp: function (evt) { return _this.keyUpHandler(evt); } }))),
            contents);
    };
    Search.prototype.rednderResults = function (files) {
        var count = files.length > 0 ? files.length : "No";
        var secs = (this.state.milliseconds / 1000).toFixed(3);
        var plural = files.length !== 1 ? "s" : "";
        var message = count + " result" + plural + " found (" + secs + " secs)";
        if (this.state.pattern === "soup" && files.length === 0)
            message = "No soup for you!";
        return React.createElement("div", { className: "resultBody" },
            React.createElement("div", { className: "smallCount" }, message),
            files.map(function (file) {
                return React.createElement(ResultFile_1.ResultFile, { key: file.full, file: file });
            }));
    };
    return Search;
}(React.Component));
exports.Search = Search;
//# sourceMappingURL=Search.js.map