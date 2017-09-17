

namespace WikiScreen.Chrome
{
    public static class ChromeJsCommands
    {

        public static string GetElementBoundsAsync(string selector)
        {
            return @"
(function(selector) { return new Promise((fulfill, reject) => {
        const element = document.querySelector(selector);

        if(element) {
            fulfill();
            return;
        }

        new MutationObserver((mutations, observer) => {
            const nodes = [];
            
            mutations.forEach((mutation) => {
                nodes.push(...mutation.addedNodes);
            });
           
            if (nodes.find((node) => node.matches(selector))) {
                observer.disconnect();
                fulfill();
            }
        }).observe(document.body, {
            childList: true
        })
    }).then(() => {
        const element = document.querySelector(selector);

        var docRect = element.ownerDocument.documentElement.getBoundingClientRect();

        const {left, top, width, height, x, y} = element.getBoundingClientRect();
        return {x: left - docRect.left , y: top - docRect.top, width, height};
    })
})('" + selector + "')";
        }
    }
}