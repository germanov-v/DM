
import {useState} from "react";

// Типы для чата, можно расширить под API
type User = {
    name: string;
    avatar: string;
    position: string;
    online?: boolean;
    lastSeen?: string;
};
type Message = {
    text?: string;
    image?: string;
    author: string;
    time: string;
    isMe?: boolean;
};

const users: User[] = [
    {
        name: "Kaiya George",
        avatar: "src/images/user/user-18.jpg",
        position: "Project Manager",
        online: true,
        lastSeen: "15 mins",
    },
    {
        name: "Lindsey Curtis",
        avatar: "src/images/user/user-17.jpg",
        position: "Designer",
        online: true,
        lastSeen: "30 mins",
    },
    // ...добавь еще пользователей
];

const messages: Message[] = [
    {
        author: "Lindsey Curtis",
        text: "I want to make an appointment tomorrow from 2:00 to 5:00pm?",
        time: "Lindsey, 2 hours ago",
    },
    {
        author: "me",
        text: "If don’t like something, I’ll stay away from it.",
        time: "2 hours ago",
        isMe: true,
    },
    {
        author: "Lindsey Curtis",
        text: "I want more detailed information.",
        time: "Lindsey, 2 hours ago",
    },
    {
        author: "me",
        text: "If don’t like something, I’ll stay away from it.",
        time: "2 hours ago",
        isMe: true,
    },
    {
        author: "me",
        text: "They got there early, and got really good seats.",
        time: "2 hours ago",
        isMe: true,
    },
    {
        author: "Lindsey Curtis",
        image: "src/images/chat/chat.jpg",
        text: "Please preview the image",
        time: "Lindsey, 2 hours ago",
    },
];

export const ChatPage: React.FC = () => {
    // Состояния дропа и сайдбара
    const [sidebarMobileOpen, setSidebarMobileOpen] = useState(false);
    const [sidebarDropdownOpen, setSidebarDropdownOpen] = useState(false);
    const [chatDropdownOpen, setChatDropdownOpen] = useState(false);
    const [input, setInput] = useState("");

    return (
        <div className="mx-auto max-w-screen-2xl p-4 md:p-6">
            {/* Breadcrumb */}
            <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
                <h2 className="text-xl font-semibold text-gray-800 dark:text-white/90">
                    Chat
                </h2>
                <nav>
                    <ol className="flex items-center gap-1.5">
                        <li>
                            <a
                                className="inline-flex items-center gap-1.5 text-sm text-gray-500 dark:text-gray-400"
                                href="index.html"
                            >
                                Home
                                <svg className="stroke-current" width="17" height="16">
                                    <path
                                        d="M6.0765 12.667L10.2432 8.50033L6.0765 4.33366"
                                        strokeWidth="1.2"
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                    />
                                </svg>
                            </a>
                        </li>
                        <li className="text-sm text-gray-800 dark:text-white/90">Chat</li>
                    </ol>
                </nav>
            </div>

            <div className="h-[calc(100vh-186px)] overflow-hidden sm:h-[calc(100vh-174px)]">
                <div className="flex h-full flex-col gap-6 xl:flex-row xl:gap-5">
                    {/* Sidebar */}
                    <div className={`flex-col rounded-2xl border border-gray-200 bg-white dark:border-gray-800 dark:bg-white/[0.03] xl:flex xl:w-1/4
            ${sidebarMobileOpen ? 'flex fixed xl:static top-0 left-0 z-50 h-screen bg-white dark:bg-gray-900' : 'hidden xl:flex'}`}>
                        <div className="sticky px-4 pb-4 pt-4 sm:px-5 sm:pt-5 xl:pb-0">
                            <div className="flex items-start justify-between">
                                <div>
                                    <h3 className="text-theme-xl font-semibold text-gray-800 dark:text-white/90 sm:text-2xl">
                                        Chats
                                    </h3>
                                </div>
                                <div className="relative">
                                    <button
                                        onClick={() => setSidebarDropdownOpen(!sidebarDropdownOpen)}
                                        className={`${
                                            sidebarDropdownOpen
                                                ? "text-gray-700 dark:text-white"
                                                : "text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-white"
                                        }`}
                                    >
                                        {/* ... */}
                                        <svg className="fill-current" width="24" height="24">
                                            <path
                                                fillRule="evenodd"
                                                clipRule="evenodd"
                                                d="M10.2441 6C10.2441 5.0335 11.0276 4.25 11.9941 4.25H12.0041C12.9706 4.25 13.7541 5.0335 13.7541 6C13.7541 6.9665 12.9706 7.75 12.0041 7.75H11.9941C11.0276 7.75 10.2441 6.9665 10.2441 6ZM10.2441 18C10.2441 17.0335 11.0276 16.25 11.9941 16.25H12.0041C12.9706 16.25 13.7541 17.0335 13.7541 18C13.7541 18.9665 12.9706 19.75 12.0041 19.75H11.9941C11.0276 19.75 10.2441 18.9665 10.2441 18ZM11.9941 10.25C11.0276 10.25 10.2441 11.0335 10.2441 12C10.2441 12.9665 11.0276 13.75 11.9941 13.75H12.0041C12.9706 13.75 13.7541 12.9665 13.7541 12C13.7541 11.0335 12.9706 10.25 12.0041 10.25H11.9941Z"
                                            />
                                        </svg>
                                    </button>
                                    {sidebarDropdownOpen && (
                                        <div className="absolute right-0 top-full z-40 w-40 space-y-1 rounded-2xl border border-gray-200 bg-white p-2 shadow-theme-lg">
                                            <button className="flex w-full rounded-lg px-3 py-2 text-left text-theme-xs font-medium text-gray-500 hover:bg-gray-100 hover:text-gray-700">
                                                View More
                                            </button>
                                            <button className="flex w-full rounded-lg px-3 py-2 text-left text-theme-xs font-medium text-gray-500 hover:bg-gray-100 hover:text-gray-700">
                                                Delete
                                            </button>
                                        </div>
                                    )}
                                </div>
                            </div>
                            <div className="mt-4 flex items-center gap-3">
                                <button
                                    onClick={() => setSidebarMobileOpen(!sidebarMobileOpen)}
                                    className="flex h-11 w-full max-w-11 items-center justify-center rounded-lg border border-gray-300 text-gray-700 xl:hidden"
                                >
                                    <svg className="fill-current" width="24" height="24">
                                        <path
                                            fillRule="evenodd"
                                            clipRule="evenodd"
                                            d="M3.25 6C3.25 5.58579 3.58579 5.25 4 5.25H20C20.4142 5.25 20.75 5.58579 20.75 6C20.75 6.41421 20.4142 6.75 20 6.75L4 6.75C3.58579 6.75 3.25 6.41422 3.25 6ZM3.25 18C3.25 17.5858 3.58579 17.25 4 17.25L20 17.25C20.4142 17.25 20.75 17.5858 20.75 18C20.75 18.4142 20.4142 18.75 20 18.75L4 18.75C3.58579 18.75 3.25 18.4142 3.25 18ZM4 11.25C3.58579 11.25 3.25 11.5858 3.25 12C3.25 12.4142 3.58579 12.75 4 12.75L20 12.75C20.4142 12.75 20.75 12.4142 20.75 12C20.75 11.5858 20.4142 11.25 20 11.25L4 11.25Z"
                                        />
                                    </svg>
                                </button>
                                <div className="relative my-2 w-full">
                                    <form>
                                        <button className="absolute left-4 top-1/2 -translate-y-1/2">
                                            <svg className="fill-gray-500" width="20" height="20">
                                                <path
                                                    fillRule="evenodd"
                                                    clipRule="evenodd"
                                                    d="M3.04199 9.37381C3.04199 5.87712 5.87735 3.04218 9.37533 3.04218C12.8733 3.04218 15.7087 5.87712 15.7087 9.37381C15.7087 12.8705 12.8733 15.7055 9.37533 15.7055C5.87735 15.7055 3.04199 12.8705 3.04199 9.37381ZM9.37533 1.54218C5.04926 1.54218 1.54199 5.04835 1.54199 9.37381C1.54199 13.6993 5.04926 17.2055 9.37533 17.2055C11.2676 17.2055 13.0032 16.5346 14.3572 15.4178L17.1773 18.2381C17.4702 18.531 17.945 18.5311 18.2379 18.2382C18.5308 17.9453 18.5309 17.4704 18.238 17.1775L15.4182 14.3575C16.5367 13.0035 17.2087 11.2671 17.2087 9.37381C17.2087 5.04835 13.7014 1.54218 9.37533 1.54218Z"
                                                />
                                            </svg>
                                        </button>
                                        <input
                                            type="text"
                                            placeholder="Search..."
                                            className="dark:bg-dark-900 h-11 w-full rounded-lg border border-gray-300 bg-transparent py-2.5 pl-[42px] pr-3.5 text-sm text-gray-800 shadow-theme-xs placeholder:text-gray-400 focus:border-brand-300 focus:outline-hidden focus:ring-3 focus:ring-brand-500/10 dark:border-gray-700 dark:bg-gray-900 dark:text-white/90 dark:placeholder:text-white/30 dark:focus:border-brand-800"
                                        />
                                    </form>
                                </div>
                            </div>
                        </div>
                        {/* Chat List */}
                        <div className="flex flex-col overflow-auto">
                            <div className="custom-scrollbar max-h-full space-y-1 overflow-auto">
                                {users.map((user) => (
                                    <div
                                        key={user.name}
                                        className="flex cursor-pointer items-center gap-3 rounded-lg p-3 hover:bg-gray-100 dark:hover:bg-white/[0.03]"
                                    >
                                        <div className="relative h-12 w-full max-w-[48px] rounded-full">
                                            <img
                                                src={user.avatar}
                                                alt="profile"
                                                className="h-full w-full overflow-hidden rounded-full object-cover object-center"
                                            />
                                            <span className={`absolute bottom-0 right-0 block h-3 w-3 rounded-full border-[1.5px] border-white ${
                                                user.online ? "bg-success-500" : "bg-gray-400"
                                            }`}></span>
                                        </div>
                                        <div className="w-full">
                                            <div className="flex items-start justify-between">
                                                <div>
                                                    <h5 className="text-sm font-medium text-gray-800 dark:text-white/90">
                                                        {user.name}
                                                    </h5>
                                                    <p className="mt-0.5 text-theme-xs text-gray-500 dark:text-gray-400">
                                                        {user.position}
                                                    </p>
                                                </div>
                                                <span className="text-theme-xs text-gray-400">
                          {user.lastSeen}
                        </span>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                    {/* Chat Box */}
                    <div className="flex h-full flex-col overflow-hidden rounded-2xl border border-gray-200 bg-white dark:border-gray-800 dark:bg-white/[0.03] xl:w-3/4">
                        <div className="sticky flex items-center justify-between border-b border-gray-200 px-5 py-4 dark:border-gray-800 xl:px-6">
                            <div className="flex items-center gap-3">
                                <div className="relative h-12 w-full max-w-[48px] rounded-full">
                                    <img
                                        src="src/images/user/user-17.jpg"
                                        alt="profile"
                                        className="h-full w-full overflow-hidden rounded-full object-cover object-center"
                                    />
                                    <span className="absolute bottom-0 right-0 block h-3 w-3 rounded-full border-[1.5px] border-white bg-success-500 dark:border-gray-900"></span>
                                </div>
                                <h5 className="text-sm font-medium text-gray-500 dark:text-gray-400">
                                    Lindsey Curtis
                                </h5>
                            </div>
                            <div className="flex items-center gap-3">
                                {/* Добавь свои кнопки (иконки) тут */}
                                <button
                                    onClick={() => setChatDropdownOpen(!chatDropdownOpen)}
                                    className={`${
                                        chatDropdownOpen
                                            ? "text-gray-800 dark:text-white/90"
                                            : "text-gray-700 dark:text-gray-400 hover:text-gray-800 dark:hover:text-white/90"
                                    }`}
                                >
                                    <svg className="fill-current" width="24" height="24">
                                        <circle cx="12" cy="6" r="2" />
                                        <circle cx="12" cy="12" r="2" />
                                        <circle cx="12" cy="18" r="2" />
                                    </svg>
                                </button>
                                {chatDropdownOpen && (
                                    <div className="absolute right-0 top-full z-40 w-40 space-y-1 rounded-2xl border border-gray-200 bg-white p-2 shadow-theme-lg">
                                        <button className="flex w-full rounded-lg px-3 py-2 text-left text-theme-xs font-medium text-gray-500 hover:bg-gray-100 hover:text-gray-700">
                                            View More
                                        </button>
                                        <button className="flex w-full rounded-lg px-3 py-2 text-left text-theme-xs font-medium text-gray-500 hover:bg-gray-100 hover:text-gray-700">
                                            Delete
                                        </button>
                                    </div>
                                )}
                            </div>
                        </div>
                        <div className="custom-scrollbar max-h-full flex-1 space-y-6 overflow-auto p-5 xl:space-y-8 xl:p-6">
                            {messages.map((m, i) =>
                                m.isMe ? (
                                    <div key={i} className="ml-auto max-w-[350px] text-right">
                                        <div className="ml-auto max-w-max rounded-lg rounded-tr-sm bg-brand-500 px-3 py-2 dark:bg-brand-500">
                                            <p className="text-sm text-white dark:text-white/90">
                                                {m.text}
                                            </p>
                                        </div>
                                        <p className="mt-2 text-theme-xs text-gray-500 dark:text-gray-400">{m.time}</p>
                                    </div>
                                ) : (
                                    <div key={i} className="max-w-[350px]">
                                        <div className="flex items-start gap-4">
                                            <div className="h-10 w-full max-w-10 rounded-full">
                                                <img src="src/images/user/user-17.jpg" alt="profile" className="h-full w-full overflow-hidden rounded-full object-cover object-center" />
                                            </div>
                                            <div>
                                                {m.image && (
                                                    <div className="mb-2 w-full max-w-[270px] overflow-hidden rounded-lg">
                                                        <img src={m.image} alt="chat" />
                                                    </div>
                                                )}
                                                <div className="rounded-lg rounded-tl-sm bg-gray-100 px-3 py-2 dark:bg-white/5">
                                                    <p className="text-sm text-gray-800 dark:text-white/90">
                                                        {m.text}
                                                    </p>
                                                </div>
                                                <p className="mt-2 text-theme-xs text-gray-500 dark:text-gray-400">{m.time}</p>
                                            </div>
                                        </div>
                                    </div>
                                )
                            )}
                        </div>
                        {/* Chat input */}
                        <div className="sticky bottom-0 border-t border-gray-200 p-3 dark:border-gray-800">
                            <form className="flex items-center justify-between">
                                <div className="relative w-full">
                                    <button type="button" className="absolute left-1 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-800 dark:text-gray-400 dark:hover:text-white/90 sm:left-3">
                                        {/* ...иконка */}
                                    </button>
                                    <input
                                        type="text"
                                        placeholder="Type a message"
                                        value={input}
                                        onChange={(e) => setInput(e.target.value)}
                                        className="h-9 w-full border-none bg-transparent pl-12 pr-5 text-sm text-gray-800 outline-hidden placeholder:text-gray-400 focus:border-0 focus:ring-0 dark:text-white/90"
                                    />
                                </div>
                                <div className="flex items-center">
                                    {/* Прочие иконки */}
                                    <button type="submit" className="ml-3 flex h-9 w-9 items-center justify-center rounded-lg bg-brand-500 text-white hover:bg-brand-600 xl:ml-5">
                                        {/* ...send иконка */}
                                    </button>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};
