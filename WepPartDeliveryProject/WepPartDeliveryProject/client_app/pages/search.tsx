import Head from 'next/head'
import { FC, useEffect, useState } from "react"
import ClientLayout from '@/components/structure/ClientLayout'
import { GetStaticProps } from 'next'
import DishMainCard from '@/components/cards/DishMainCard'
import { useRouter } from 'next/router'

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories
        }
    }
}

const Search: FC<{ categories: categoryItem[] }> = ({ categories }) => {
    const [dishes, setDishes] = useState<dishClientInfo[]>([]);
    //нулевая страница загружается при переходе на страницу
    const [page, setPage] = useState(1);
    const [pageEnded, setPageEnded] = useState(false);
    
    const router = useRouter();
    const searchTextFromParam = router.query.searchText;

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getSearchedDishes?searchText=${searchTextFromParam}`);
            const loadedData = await resp.json() as {dishes: dishClientInfo[], pageEnded: boolean};

            setDishes(loadedData.dishes);

            setPageEnded(loadedData.pageEnded);
        }
        fetchData();
    }, [searchTextFromParam])

    const handleShowMoreDishes = async () => {
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getSearchedDishes?searchText=${searchTextFromParam}&page=${page}`);
        const loadedData = await resp.json() as {dishes: dishClientInfo[], pageEnded: boolean};

        if(resp.ok){
            setPage(page + 1);
            setDishes(dishes.concat(loadedData.dishes));
        }

        setPageEnded(loadedData.pageEnded);
    }

    return (
        <ClientLayout categories={categories}>
            <Head>
                <title>Create Next App</title>
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <link rel="icon" href="/favicon.ico" />
            </Head>
            <main className='mb-2'>
                <div>
                    {dishes.map((dish, i) => <DishMainCard key={i} {...dish} />)}
                </div>
                {dishes.length != 0 ?
                    (!pageEnded && (<div>
                        <button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreDishes}>
                            Показать больше
                        </button>
                    </div>))
                    : (
                        <div className='d-flex justify-content-center mt-5'>
                            <h2>
                                В результате поиска не нашлось ни одного подходящего блюда
                            </h2>
                        </div>
                    )
                }
            </main>
        </ClientLayout>
    )
}

export default Search;