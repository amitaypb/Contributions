<?php
// src/Controller/LuckyController.php
namespace App\Controller;

use Symfony\Component\Routing\Annotation\Route;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Bundle\FrameworkBundle\Controller\AbstractController;

class LuckyController extends AbstractController
{
	/**
     * @Route("/lucky/number")
     */
    public function number()
    {
        $number = random_int(0, 100);

		//If you're returning HTML from your controller, you'll probably want to render a template. 
		//Fortunately, Symfony comes with Twig: a templating language that's easy, powerful and actually quite fun.
        //return new Response(
        //    '<html><body>Lucky number: '.$number.'</body></html>'
        //);
		return $this->render('lucky/number.html.twig', [
            'number' => $number,
        ]);
    }
	
	/**
     * @Route("/lucky/number/{number}")
     */
	public function print_number(int $number)
	{
		return $this->render('lucky/number.html.twig', [
            'number' => $number,
        ]);
	}
}
