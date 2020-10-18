using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using System;
using System.Threading.Tasks;

namespace Espeon.Menus {
    public class DeleteOnReaction : MenuBase {
        private readonly Func<Task<IUserMessage>> _messageFunc;

        public DeleteOnReaction(Func<Task<IUserMessage>> messageFunc) {
            this._messageFunc = messageFunc;

            AddButtonAsync(new Button(new LocalEmoji("🚮"), async args => {
                _ = Message.DeleteAsync();
                await StopAsync();
            }));
        }

        protected override async Task<IUserMessage> InitialiseAsync() {
            return await this._messageFunc();
        }
    }
}